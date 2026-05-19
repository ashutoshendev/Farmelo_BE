using Farmelo.Business.Commands.Business;
using Farmelo.Business.Services.Invoices;
using Farmelo.Business.Support;
using Farmelo.Data.Write.Abstractions;
using Farmelo.Data.Write.EFContext;
using Farmelo.Data.Write.Entities;
using Farmelo.Shared.DTO.Business;
using Farmelo.Shared.OperationResult;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Farmelo.Business.Handlers.Business;

public sealed class BusinessCommandHandler :
    IRequestHandler<CreatePartyCommand, ServiceOperationResult<PartyDto>>,
    IRequestHandler<UpdatePartyCommand, ServiceOperationResult<PartyDto>>,
    IRequestHandler<AddRawStockCommand, ServiceOperationResult<RawStockEntryDto>>,
    IRequestHandler<CreateBoxProductionCommand, ServiceOperationResult<BoxStockSummaryDto>>,
    IRequestHandler<CreateB2BOrderCommand, ServiceOperationResult<B2BOrderDto>>,
    IRequestHandler<CreateB2CAssignmentCommand, ServiceOperationResult<B2CAssignmentDto>>,
    IRequestHandler<ReturnB2CStockCommand, ServiceOperationResult<B2CAssignmentDto>>,
    IRequestHandler<CreatePaymentCommand, ServiceOperationResult<PaymentDto>>
{
    private const int DefaultPaymentDueDays = 30;
    private readonly FarmeloDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IInvoiceService _invoiceService;

    public BusinessCommandHandler(FarmeloDbContext dbContext, ICurrentUser currentUser, IInvoiceService invoiceService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _invoiceService = invoiceService;
    }

    public async Task<ServiceOperationResult<PartyDto>> Handle(CreatePartyCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var party = new Party
        {
            PartyType = NormalizePartyType(request.Request.PartyType),
            Name = request.Request.Name.Trim(),
            ContactName = TextNormalizer.NullIfWhiteSpace(request.Request.ContactName),
            Phone = TextNormalizer.NullIfWhiteSpace(request.Request.Phone),
            Email = TextNormalizer.NullIfWhiteSpace(request.Request.Email),
            Location = TextNormalizer.NullIfWhiteSpace(request.Request.Location),
            IsActive = request.Request.IsActive,
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        };

        _dbContext.Parties.Add(party);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ServiceOperationResult.CreateWithSuccess(ToPartyDto(party), "Party created.");
    }

    public async Task<ServiceOperationResult<PartyDto>> Handle(UpdatePartyCommand request, CancellationToken cancellationToken)
    {
        var party = await _dbContext.Parties.AsTracking().FirstOrDefaultAsync(x => x.Id == request.PartyId, cancellationToken);
        if (party == null)
        {
            return ServiceOperationResult.CreateWithFailure<PartyDto>("Party was not found.");
        }

        party.PartyType = NormalizePartyType(request.Request.PartyType);
        party.Name = request.Request.Name.Trim();
        party.ContactName = TextNormalizer.NullIfWhiteSpace(request.Request.ContactName);
        party.Phone = TextNormalizer.NullIfWhiteSpace(request.Request.Phone);
        party.Email = TextNormalizer.NullIfWhiteSpace(request.Request.Email);
        party.Location = TextNormalizer.NullIfWhiteSpace(request.Request.Location);
        party.IsActive = request.Request.IsActive;
        party.ModifiedBy = CurrentUserName();
        party.ModifiedOn = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ServiceOperationResult.CreateWithSuccess(ToPartyDto(party), "Party updated.");
    }

    public async Task<ServiceOperationResult<RawStockEntryDto>> Handle(AddRawStockCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var entryDate = request.Request.EntryDate ?? now;
        var sellerName = TextNormalizer.NullIfWhiteSpace(request.Request.SupplierName);
        if (request.Request.SellerPartyId.HasValue)
        {
            var seller = await _dbContext.Parties.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.Request.SellerPartyId.Value && x.IsActive && x.PartyType == "Seller",
                    cancellationToken);
            if (seller == null)
            {
                return ServiceOperationResult.CreateWithFailure<RawStockEntryDto>("Active seller was not found.");
            }

            sellerName = seller.Name;
        }

        var entry = new RawStockEntry
        {
            SellerPartyId = request.Request.SellerPartyId,
            EntryDate = entryDate,
            SuttaGrade = NormalizeSuttaGrade(request.Request.SuttaGrade),
            QuantityKg = request.Request.QuantityKg,
            AvailableKg = request.Request.QuantityKg,
            CostPerKg = request.Request.CostPerKg,
            SupplierName = sellerName,
            Notes = TextNormalizer.NullIfWhiteSpace(request.Request.Notes),
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        };

        _dbContext.RawStockEntries.Add(entry);
        _dbContext.RawStockMovements.Add(new RawStockMovement
        {
            MovementDate = entryDate,
            MovementType = "IN",
            SuttaGrade = entry.SuttaGrade,
            QuantityKg = request.Request.QuantityKg,
            CostPerKg = request.Request.CostPerKg,
            ReferenceType = "RawStockEntry",
            Notes = entry.Notes,
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ServiceOperationResult.CreateWithSuccess(ToRawStockEntryDto(entry), "Raw stock added.");
    }

    public async Task<ServiceOperationResult<BoxStockSummaryDto>> Handle(
        CreateBoxProductionCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Request.ProductId, cancellationToken);
        if (product == null)
        {
            return ServiceOperationResult.CreateWithFailure<BoxStockSummaryDto>("Product was not found.");
        }

        if (product.WeightGrams <= 0)
        {
            return ServiceOperationResult.CreateWithFailure<BoxStockSummaryDto>("Product weight in grams is required before creating boxes.");
        }

        var requiredKg = Math.Round(product.WeightGrams * request.Request.Quantity / 1000m, 3);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var suttaGrade = NormalizeSuttaGrade(request.Request.SuttaGrade);
        var costPerKg = await DeductRawStockAsync(requiredKg, suttaGrade, cancellationToken);
        if (costPerKg == null)
        {
            return ServiceOperationResult.CreateWithFailure<BoxStockSummaryDto>($"Insufficient raw stock for {suttaGrade}.");
        }

        var now = DateTime.UtcNow;
        var productionDate = request.Request.ProductionDate ?? now;
        var unitCost = Math.Round(costPerKg.Value * product.WeightGrams / 1000m, 2);
        _dbContext.RawStockMovements.Add(new RawStockMovement
        {
            MovementDate = productionDate,
            MovementType = "BOX_CONVERSION",
            SuttaGrade = suttaGrade,
            QuantityKg = -requiredKg,
            CostPerKg = costPerKg.Value,
            ReferenceType = "BoxProduction",
            Notes = request.Request.Notes,
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        });
        _dbContext.BoxStockMovements.Add(new BoxStockMovement
        {
            ProductId = product.Id,
            MovementDate = productionDate,
            MovementType = "CREATED",
            SuttaGrade = suttaGrade,
            Quantity = request.Request.Quantity,
            UnitCost = unitCost,
            UnitPrice = product.CurrentPrice,
            ReferenceType = "BoxProduction",
            Notes = TextNormalizer.NullIfWhiteSpace(request.Request.Notes),
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        var summary = await GetBoxStockSummaryAsync(product.Id, cancellationToken);
        return ServiceOperationResult.CreateWithSuccess(summary!, "Box stock created.");
    }

    public async Task<ServiceOperationResult<B2BOrderDto>> Handle(CreateB2BOrderCommand request, CancellationToken cancellationToken)
    {
        var party = await _dbContext.Parties.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Request.PartyId && x.IsActive, cancellationToken);
        if (party == null || party.PartyType != "B2B")
        {
            return ServiceOperationResult.CreateWithFailure<B2BOrderDto>("Active B2B buyer was not found.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var suttaGrade = NormalizeSuttaGrade(request.Request.SuttaGrade);
        var costPerKg = await DeductRawStockAsync(request.Request.QuantityKg, suttaGrade, cancellationToken);
        if (costPerKg == null)
        {
            return ServiceOperationResult.CreateWithFailure<B2BOrderDto>($"Insufficient raw stock for {suttaGrade}.");
        }

        var now = DateTime.UtcNow;
        var orderDate = request.Request.OrderDate ?? now;
        var totalValue = Math.Round(request.Request.QuantityKg * request.Request.PricePerKg, 2);
        var totalCost = Math.Round(request.Request.QuantityKg * costPerKg.Value, 2);
        var order = new B2BOrder
        {
            PartyId = party.Id,
            OrderDate = orderDate,
            DueDate = request.Request.DueDate ?? orderDate.AddDays(DefaultPaymentDueDays),
            SuttaGrade = suttaGrade,
            QuantityKg = request.Request.QuantityKg,
            PricePerKg = request.Request.PricePerKg,
            TotalValue = totalValue,
            CostPerKg = costPerKg.Value,
            TotalCost = totalCost,
            Margin = totalValue - totalCost,
            Notes = TextNormalizer.NullIfWhiteSpace(request.Request.Notes),
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        };

        _dbContext.B2BOrders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _dbContext.RawStockMovements.Add(new RawStockMovement
        {
            MovementDate = orderDate,
            MovementType = "B2B_OUT",
            SuttaGrade = order.SuttaGrade,
            QuantityKg = -order.QuantityKg,
            CostPerKg = order.CostPerKg,
            ReferenceType = "B2BOrder",
            ReferenceId = order.Id,
            Notes = order.Notes,
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        var invoice = await CreateB2BInvoiceSafelyAsync(order.Id, cancellationToken);
        return ServiceOperationResult.CreateWithSuccess(ToB2BOrderDto(order, party.Name, 0m, invoice), "B2B order created.");
    }

    public async Task<ServiceOperationResult<B2CAssignmentDto>> Handle(
        CreateB2CAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var party = await _dbContext.Parties.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Request.PartyId && x.IsActive, cancellationToken);
        if (party == null || (party.PartyType != "Shop" && party.PartyType != "Distributor"))
        {
            return ServiceOperationResult.CreateWithFailure<B2CAssignmentDto>("Active shop or distributor was not found.");
        }

        var product = await _dbContext.Products.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Request.ProductId && x.IsActive, cancellationToken);
        if (product == null)
        {
            return ServiceOperationResult.CreateWithFailure<B2CAssignmentDto>("Active product was not found.");
        }

        var stock = await GetBoxStockSummaryAsync(product.Id, cancellationToken);
        if (stock == null || stock.AvailableQuantity < request.Request.Quantity)
        {
            return ServiceOperationResult.CreateWithFailure<B2CAssignmentDto>("Insufficient box stock.");
        }

        var now = DateTime.UtcNow;
        var assignmentDate = request.Request.AssignmentDate ?? now;
        var unitPrice = request.Request.UnitPrice ?? product.CurrentPrice;
        var totalValue = Math.Round(unitPrice * request.Request.Quantity, 2);
        var totalCost = Math.Round(stock.AverageUnitCost * request.Request.Quantity, 2);
        var assignment = new B2CAssignment
        {
            PartyId = party.Id,
            ProductId = product.Id,
            AssignmentDate = assignmentDate,
            DueDate = request.Request.DueDate ?? assignmentDate.AddDays(DefaultPaymentDueDays),
            Quantity = request.Request.Quantity,
            UnitPrice = unitPrice,
            TotalValue = totalValue,
            UnitCost = stock.AverageUnitCost,
            TotalCost = totalCost,
            Margin = totalValue - totalCost,
            Notes = TextNormalizer.NullIfWhiteSpace(request.Request.Notes),
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.B2CAssignments.Add(assignment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _dbContext.BoxStockMovements.Add(new BoxStockMovement
        {
            ProductId = product.Id,
            MovementDate = assignmentDate,
            MovementType = "ASSIGNED",
            SuttaGrade = string.Empty,
            Quantity = request.Request.Quantity,
            UnitCost = assignment.UnitCost,
            UnitPrice = assignment.UnitPrice,
            ReferenceType = "B2CAssignment",
            ReferenceId = assignment.Id,
            Notes = assignment.Notes,
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        var invoice = await CreateB2CInvoiceSafelyAsync(assignment.Id, cancellationToken);
        return ServiceOperationResult.CreateWithSuccess(ToB2CAssignmentDto(assignment, party.Name, product.Name, 0m, invoice), "B2C stock assigned.");
    }

    public async Task<ServiceOperationResult<B2CAssignmentDto>> Handle(
        ReturnB2CStockCommand request,
        CancellationToken cancellationToken)
    {
        var assignment = await _dbContext.B2CAssignments.AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Request.AssignmentId, cancellationToken);
        if (assignment == null)
        {
            return ServiceOperationResult.CreateWithFailure<B2CAssignmentDto>("B2C assignment was not found.");
        }

        if (assignment.ReturnedQuantity + request.Request.Quantity > assignment.Quantity)
        {
            return ServiceOperationResult.CreateWithFailure<B2CAssignmentDto>("Return quantity cannot exceed assigned quantity.");
        }

        var paid = await _dbContext.Payments
            .Where(x => x.B2CAssignmentId == assignment.Id)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
        var netQuantity = assignment.Quantity - assignment.ReturnedQuantity - request.Request.Quantity;
        var newTotalValue = Math.Round(netQuantity * assignment.UnitPrice, 2);
        if (paid > newTotalValue)
        {
            return ServiceOperationResult.CreateWithFailure<B2CAssignmentDto>("Return would make collected payment greater than assignment value.");
        }

        var partyName = await _dbContext.Parties.Where(x => x.Id == assignment.PartyId).Select(x => x.Name).FirstAsync(cancellationToken);
        var productName = await _dbContext.Products.Where(x => x.Id == assignment.ProductId).Select(x => x.Name).FirstAsync(cancellationToken);
        var now = DateTime.UtcNow;
        assignment.ReturnedQuantity += request.Request.Quantity;
        assignment.TotalValue = newTotalValue;
        assignment.TotalCost = Math.Round(netQuantity * assignment.UnitCost, 2);
        assignment.Margin = assignment.TotalValue - assignment.TotalCost;
        assignment.ModifiedBy = CurrentUserName();
        assignment.ModifiedOn = now;

        _dbContext.BoxStockMovements.Add(new BoxStockMovement
        {
            ProductId = assignment.ProductId,
            MovementDate = request.Request.ReturnDate ?? now,
            MovementType = "RETURNED",
            SuttaGrade = string.Empty,
            Quantity = request.Request.Quantity,
            UnitCost = assignment.UnitCost,
            UnitPrice = assignment.UnitPrice,
            ReferenceType = "B2CAssignment",
            ReferenceId = assignment.Id,
            Notes = TextNormalizer.NullIfWhiteSpace(request.Request.Notes),
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ServiceOperationResult.CreateWithSuccess(ToB2CAssignmentDto(assignment, partyName, productName, paid), "B2C stock returned.");
    }

    public async Task<ServiceOperationResult<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var paymentType = request.Request.PaymentType.Trim().ToUpperInvariant();
        var now = DateTime.UtcNow;
        int partyId;

        if (paymentType == "B2B")
        {
            var order = await _dbContext.B2BOrders.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Request.B2BOrderId, cancellationToken);
            if (order == null)
            {
                return ServiceOperationResult.CreateWithFailure<PaymentDto>("B2B order was not found.");
            }

            var paid = await _dbContext.Payments.Where(x => x.B2BOrderId == order.Id).SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
            if (request.Request.Amount > order.TotalValue - paid)
            {
                return ServiceOperationResult.CreateWithFailure<PaymentDto>("Payment amount cannot exceed pending amount.");
            }

            partyId = order.PartyId;
        }
        else
        {
            var assignment = await _dbContext.B2CAssignments.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Request.B2CAssignmentId, cancellationToken);
            if (assignment == null)
            {
                return ServiceOperationResult.CreateWithFailure<PaymentDto>("B2C assignment was not found.");
            }

            var paid = await _dbContext.Payments.Where(x => x.B2CAssignmentId == assignment.Id).SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
            if (request.Request.Amount > assignment.TotalValue - paid)
            {
                return ServiceOperationResult.CreateWithFailure<PaymentDto>("Payment amount cannot exceed pending amount.");
            }

            partyId = assignment.PartyId;
        }

        var payment = new Payment
        {
            PartyId = partyId,
            PaymentType = paymentType,
            B2BOrderId = paymentType == "B2B" ? request.Request.B2BOrderId : null,
            B2CAssignmentId = paymentType == "B2C" ? request.Request.B2CAssignmentId : null,
            PaymentDate = request.Request.PaymentDate ?? now,
            Amount = request.Request.Amount,
            PaymentMode = TextNormalizer.NullIfWhiteSpace(request.Request.PaymentMode),
            ReferenceNumber = TextNormalizer.NullIfWhiteSpace(request.Request.ReferenceNumber),
            Notes = TextNormalizer.NullIfWhiteSpace(request.Request.Notes),
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var partyName = await _dbContext.Parties.Where(x => x.Id == partyId).Select(x => x.Name).FirstAsync(cancellationToken);
        return ServiceOperationResult.CreateWithSuccess(ToPaymentDto(payment, partyName), "Payment recorded.");
    }

    private async Task<decimal?> DeductRawStockAsync(decimal quantityKg, string suttaGrade, CancellationToken cancellationToken)
    {
        var entries = await _dbContext.RawStockEntries.AsTracking()
            .Where(x => x.AvailableKg > 0 && x.SuttaGrade == suttaGrade)
            .OrderBy(x => x.EntryDate)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
        var totalAvailable = entries.Sum(x => x.AvailableKg);

        if (totalAvailable < quantityKg)
        {
            return null;
        }

        var remaining = quantityKg;
        var consumedValue = 0m;

        foreach (var entry in entries)
        {
            if (remaining <= 0)
            {
                break;
            }

            var consumed = Math.Min(entry.AvailableKg, remaining);
            entry.AvailableKg -= consumed;
            consumedValue += consumed * entry.CostPerKg;
            entry.ModifiedBy = CurrentUserName();
            entry.ModifiedOn = DateTime.UtcNow;
            remaining -= consumed;
        }

        return Math.Round(consumedValue / quantityKg, 2);
    }

    private async Task<BoxStockSummaryDto?> GetBoxStockSummaryAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);
        if (product == null)
        {
            return null;
        }

        var movements = await _dbContext.BoxStockMovements.AsNoTracking()
            .Where(x => x.ProductId == productId)
            .ToListAsync(cancellationToken);
        var available = movements.Sum(SignedBoxQuantity);
        var value = movements.Sum(x => SignedBoxQuantity(x) * x.UnitCost);

        return new BoxStockSummaryDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Weight = product.Weight,
            WeightGrams = product.WeightGrams,
            AvailableQuantity = available,
            AverageUnitCost = available > 0 ? Math.Round(value / available, 2) : 0m,
            CurrentPrice = product.CurrentPrice
        };
    }

    private static int SignedBoxQuantity(BoxStockMovement movement)
        => movement.MovementType == "ASSIGNED" ? -movement.Quantity : movement.Quantity;

    private async Task<InvoiceDto?> CreateB2BInvoiceSafelyAsync(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            return await _invoiceService.CreateForB2BOrderAsync(orderId, cancellationToken);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<InvoiceDto?> CreateB2CInvoiceSafelyAsync(int assignmentId, CancellationToken cancellationToken)
    {
        try
        {
            return await _invoiceService.CreateForB2CAssignmentAsync(assignmentId, cancellationToken);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private string CurrentUserName()
        => TextNormalizer.CurrentUserNameOrSystem(_currentUser.UserName);

    private static string NormalizePartyType(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Equals("B2B", StringComparison.OrdinalIgnoreCase))
        {
            return "B2B";
        }

        if (trimmed.Equals("Shop", StringComparison.OrdinalIgnoreCase))
        {
            return "Shop";
        }

        if (trimmed.Equals("Seller", StringComparison.OrdinalIgnoreCase))
        {
            return "Seller";
        }

        return "Distributor";
    }

    private static string NormalizeSuttaGrade(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Equals("Makhana 4 Plus Sutta", StringComparison.OrdinalIgnoreCase)
            ? "Makhana 4 Plus Sutta"
            : trimmed.Equals("Makhana 5 Sutta", StringComparison.OrdinalIgnoreCase)
                ? "Makhana 5 Sutta"
                : trimmed.Equals("Makhana 5 Plus Sutta", StringComparison.OrdinalIgnoreCase)
                    ? "Makhana 5 Plus Sutta"
                    : trimmed.Equals("Makhana 6 Sutta", StringComparison.OrdinalIgnoreCase)
                        ? "Makhana 6 Sutta"
                        : trimmed.Equals("Makhana 6 Plus Sutta", StringComparison.OrdinalIgnoreCase)
                            ? "Makhana 6 Plus Sutta"
                            : "Makhana 4 Sutta";
    }

    private static PartyDto ToPartyDto(Party party)
        => new()
        {
            Id = party.Id,
            PartyType = party.PartyType,
            Name = party.Name,
            ContactName = party.ContactName,
            Phone = party.Phone,
            Email = party.Email,
            Location = party.Location,
            IsActive = party.IsActive,
            CreatedOn = party.CreatedOn,
            ModifiedOn = party.ModifiedOn
        };

    private static RawStockEntryDto ToRawStockEntryDto(RawStockEntry entry)
        => new()
        {
            Id = entry.Id,
            SellerPartyId = entry.SellerPartyId,
            SellerName = entry.SupplierName,
            EntryDate = entry.EntryDate,
            SuttaGrade = entry.SuttaGrade,
            QuantityKg = entry.QuantityKg,
            AvailableKg = entry.AvailableKg,
            CostPerKg = entry.CostPerKg,
            TotalCost = Math.Round(entry.QuantityKg * entry.CostPerKg, 2),
            SupplierName = entry.SupplierName,
            Notes = entry.Notes
        };

    private static B2BOrderDto ToB2BOrderDto(B2BOrder order, string partyName, decimal paid, InvoiceDto? invoice = null)
        => new()
        {
            Id = order.Id,
            PartyId = order.PartyId,
            PartyName = partyName,
            OrderDate = order.OrderDate,
            DueDate = order.DueDate,
            SuttaGrade = order.SuttaGrade,
            QuantityKg = order.QuantityKg,
            PricePerKg = order.PricePerKg,
            TotalValue = order.TotalValue,
            CostPerKg = order.CostPerKg,
            TotalCost = order.TotalCost,
            Margin = order.Margin,
            PaidAmount = paid,
            PendingAmount = order.TotalValue - paid,
            IsOverdue = order.TotalValue > paid && order.DueDate.Date < DateTime.UtcNow.AddHours(5.5).Date,
            Notes = order.Notes,
            InvoiceId = invoice?.Id,
            InvoiceNumber = invoice?.InvoiceNumber,
            InvoiceEmailStatus = invoice?.EmailStatus,
            InvoiceDownloadUrl = invoice?.DownloadUrl,
            Invoice = invoice
        };

    private static B2CAssignmentDto ToB2CAssignmentDto(
        B2CAssignment assignment,
        string partyName,
        string productName,
        decimal paid,
        InvoiceDto? invoice = null)
        => new()
        {
            Id = assignment.Id,
            PartyId = assignment.PartyId,
            PartyName = partyName,
            ProductId = assignment.ProductId,
            ProductName = productName,
            AssignmentDate = assignment.AssignmentDate,
            DueDate = assignment.DueDate,
            Quantity = assignment.Quantity,
            ReturnedQuantity = assignment.ReturnedQuantity,
            UnitPrice = assignment.UnitPrice,
            TotalValue = assignment.TotalValue,
            UnitCost = assignment.UnitCost,
            TotalCost = assignment.TotalCost,
            Margin = assignment.Margin,
            PaidAmount = paid,
            PendingAmount = assignment.TotalValue - paid,
            IsOverdue = assignment.TotalValue > paid && assignment.DueDate.Date < DateTime.UtcNow.AddHours(5.5).Date,
            Notes = assignment.Notes,
            InvoiceId = invoice?.Id,
            InvoiceNumber = invoice?.InvoiceNumber,
            InvoiceEmailStatus = invoice?.EmailStatus,
            InvoiceDownloadUrl = invoice?.DownloadUrl,
            Invoice = invoice
        };

    private static PaymentDto ToPaymentDto(Payment payment, string partyName)
        => new()
        {
            Id = payment.Id,
            PartyId = payment.PartyId,
            PartyName = partyName,
            PaymentType = payment.PaymentType,
            B2BOrderId = payment.B2BOrderId,
            B2CAssignmentId = payment.B2CAssignmentId,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            PaymentMode = payment.PaymentMode,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes
        };
}
