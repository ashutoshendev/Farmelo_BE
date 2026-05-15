using Dapper;
using Farmelo.Business.Queries.Business;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Shared.DTO.Business;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Business;

public sealed class BusinessQueryHandler :
    IRequestHandler<GetPartiesQuery, ServiceOperationResult<IReadOnlyList<PartyDto>>>,
    IRequestHandler<GetRawStockEntriesQuery, ServiceOperationResult<IReadOnlyList<RawStockEntryDto>>>,
    IRequestHandler<GetInventorySummaryQuery, ServiceOperationResult<InventorySummaryDto>>,
    IRequestHandler<GetB2BOrdersQuery, ServiceOperationResult<IReadOnlyList<B2BOrderDto>>>,
    IRequestHandler<GetB2CAssignmentsQuery, ServiceOperationResult<IReadOnlyList<B2CAssignmentDto>>>,
    IRequestHandler<GetPaymentsQuery, ServiceOperationResult<IReadOnlyList<PaymentDto>>>,
    IRequestHandler<GetPaymentSummaryQuery, ServiceOperationResult<PaymentSummaryDto>>,
    IRequestHandler<GetBusinessDashboardQuery, ServiceOperationResult<BusinessDashboardDto>>,
    IRequestHandler<GetMonthlySummaryQuery, ServiceOperationResult<MonthlySummaryDto>>,
    IRequestHandler<GetPurchaseReportQuery, ServiceOperationResult<PurchaseReportDto>>,
    IRequestHandler<GetSalesReportQuery, ServiceOperationResult<SalesReportDto>>,
    IRequestHandler<GetProfitLossReportQuery, ServiceOperationResult<ProfitLossReportDto>>,
    IRequestHandler<GetStockValuationReportQuery, ServiceOperationResult<StockValuationReportDto>>
{
    private readonly IDapperExecutor _dapper;

    public BusinessQueryHandler(IDapperExecutor dapper)
    {
        _dapper = dapper;
    }

    public async Task<ServiceOperationResult<IReadOnlyList<PartyDto>>> Handle(GetPartiesQuery request, CancellationToken cancellationToken)
    {
        var partyType = string.IsNullOrWhiteSpace(request.PartyType) ? null : request.PartyType.Trim();
        var parties = await _dapper.QueryAsync<PartyDto>(
            """
            SELECT Id, PartyType, Name, ContactName, Phone, Email, Location, IsActive, CreatedOn, ModifiedOn
            FROM Parties
            WHERE (@PartyType IS NULL OR PartyType = @PartyType)
            ORDER BY IsActive DESC, PartyType, Name
            """,
            new { PartyType = partyType },
            ct: cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(parties);
    }

    public async Task<ServiceOperationResult<IReadOnlyList<RawStockEntryDto>>> Handle(
        GetRawStockEntriesQuery request,
        CancellationToken cancellationToken)
    {
        var entries = await _dapper.QueryAsync<RawStockEntryDto>(
            """
            SELECT r.Id, r.SellerPartyId, COALESCE(s.Name, r.SupplierName) AS SellerName,
                   r.EntryDate, r.SuttaGrade, r.QuantityKg, r.AvailableKg, r.CostPerKg,
                   r.QuantityKg * r.CostPerKg AS TotalCost, r.SupplierName, r.Notes
            FROM RawStockEntries r
            LEFT JOIN Parties s ON s.Id = r.SellerPartyId
            ORDER BY EntryDate DESC, Id DESC
            """,
            ct: cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(entries);
    }

    public async Task<ServiceOperationResult<InventorySummaryDto>> Handle(
        GetInventorySummaryQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await BuildInventorySummaryAsync(cancellationToken);
        return ServiceOperationResult.CreateWithSuccess(summary);
    }

    public async Task<ServiceOperationResult<IReadOnlyList<B2BOrderDto>>> Handle(
        GetB2BOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _dapper.QueryAsync<B2BOrderDto>(
            """
            WITH PaymentTotals AS (
                SELECT B2BOrderId, SUM(Amount) AS PaidAmount
                FROM Payments
                WHERE B2BOrderId IS NOT NULL
                GROUP BY B2BOrderId
            )
            SELECT o.Id, o.PartyId, p.Name AS PartyName, o.OrderDate, o.DueDate, o.SuttaGrade, o.QuantityKg, o.PricePerKg,
                   o.TotalValue, o.CostPerKg, o.TotalCost, o.Margin,
                   ISNULL(pt.PaidAmount, 0) AS PaidAmount,
                   o.TotalValue - ISNULL(pt.PaidAmount, 0) AS PendingAmount,
                   CAST(CASE WHEN o.TotalValue > ISNULL(pt.PaidAmount, 0) AND CAST(o.DueDate AS date) < @Today THEN 1 ELSE 0 END AS bit) AS IsOverdue,
                   o.Notes,
                   i.Id AS InvoiceId, i.InvoiceNumber, i.EmailStatus AS InvoiceEmailStatus,
                   CASE WHEN i.Id IS NULL THEN NULL ELSE CONCAT('/api/invoices/', i.Id, '/download') END AS InvoiceDownloadUrl
            FROM B2BOrders o
            INNER JOIN Parties p ON p.Id = o.PartyId
            LEFT JOIN PaymentTotals pt ON pt.B2BOrderId = o.Id
            LEFT JOIN Invoices i ON i.B2BOrderId = o.Id
            WHERE (@PartyId IS NULL OR o.PartyId = @PartyId)
            ORDER BY o.OrderDate DESC, o.Id DESC
            """,
            new { request.PartyId, Today = TodayInIndia() },
            ct: cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(orders);
    }

    public async Task<ServiceOperationResult<IReadOnlyList<B2CAssignmentDto>>> Handle(
        GetB2CAssignmentsQuery request,
        CancellationToken cancellationToken)
    {
        var assignments = await _dapper.QueryAsync<B2CAssignmentDto>(
            """
            WITH PaymentTotals AS (
                SELECT B2CAssignmentId, SUM(Amount) AS PaidAmount
                FROM Payments
                WHERE B2CAssignmentId IS NOT NULL
                GROUP BY B2CAssignmentId
            )
            SELECT a.Id, a.PartyId, p.Name AS PartyName, a.ProductId, pr.Name AS ProductName,
                   a.AssignmentDate, a.DueDate, a.Quantity, a.ReturnedQuantity, a.UnitPrice,
                   a.TotalValue, a.UnitCost, a.TotalCost, a.Margin,
                   ISNULL(pt.PaidAmount, 0) AS PaidAmount,
                   a.TotalValue - ISNULL(pt.PaidAmount, 0) AS PendingAmount,
                   CAST(CASE WHEN a.TotalValue > ISNULL(pt.PaidAmount, 0) AND CAST(a.DueDate AS date) < @Today THEN 1 ELSE 0 END AS bit) AS IsOverdue,
                   a.Notes,
                   i.Id AS InvoiceId, i.InvoiceNumber, i.EmailStatus AS InvoiceEmailStatus,
                   CASE WHEN i.Id IS NULL THEN NULL ELSE CONCAT('/api/invoices/', i.Id, '/download') END AS InvoiceDownloadUrl
            FROM B2CAssignments a
            INNER JOIN Parties p ON p.Id = a.PartyId
            INNER JOIN Products pr ON pr.Id = a.ProductId
            LEFT JOIN PaymentTotals pt ON pt.B2CAssignmentId = a.Id
            LEFT JOIN Invoices i ON i.B2CAssignmentId = a.Id
            WHERE (@PartyId IS NULL OR a.PartyId = @PartyId)
            ORDER BY a.AssignmentDate DESC, a.Id DESC
            """,
            new { request.PartyId, Today = TodayInIndia() },
            ct: cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(assignments);
    }

    public async Task<ServiceOperationResult<IReadOnlyList<PaymentDto>>> Handle(
        GetPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await _dapper.QueryAsync<PaymentDto>(
            """
            SELECT pay.Id, pay.PartyId, p.Name AS PartyName, pay.PaymentType, pay.B2BOrderId, pay.B2CAssignmentId,
                   pay.PaymentDate, pay.Amount, pay.PaymentMode, pay.ReferenceNumber, pay.Notes
            FROM Payments pay
            INNER JOIN Parties p ON p.Id = pay.PartyId
            WHERE (@PartyId IS NULL OR pay.PartyId = @PartyId)
            ORDER BY pay.PaymentDate DESC, pay.Id DESC
            """,
            new { request.PartyId },
            ct: cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(payments);
    }

    public async Task<ServiceOperationResult<PaymentSummaryDto>> Handle(
        GetPaymentSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await BuildPaymentSummaryAsync(cancellationToken);
        return ServiceOperationResult.CreateWithSuccess(summary);
    }

    public async Task<ServiceOperationResult<BusinessDashboardDto>> Handle(
        GetBusinessDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var inventory = await BuildInventorySummaryAsync(cancellationToken);
        var payments = await BuildPaymentSummaryAsync(cancellationToken);
        var dashboard = await _dapper.QuerySingleAsync<BusinessDashboardDto>(
            """
            WITH B2BPayments AS (
                SELECT B2BOrderId, SUM(Amount) PaidAmount FROM Payments WHERE B2BOrderId IS NOT NULL GROUP BY B2BOrderId
            ),
            B2CPayments AS (
                SELECT B2CAssignmentId, SUM(Amount) PaidAmount FROM Payments WHERE B2CAssignmentId IS NOT NULL GROUP BY B2CAssignmentId
            )
            SELECT
                (SELECT COUNT(1) FROM B2BOrders o LEFT JOIN B2BPayments p ON p.B2BOrderId = o.Id WHERE o.TotalValue > ISNULL(p.PaidAmount, 0)) AS ActiveB2BOrders,
                (SELECT ISNULL(SUM(o.TotalValue - ISNULL(p.PaidAmount, 0)), 0) FROM B2BOrders o LEFT JOIN B2BPayments p ON p.B2BOrderId = o.Id) AS B2BPending,
                (SELECT COUNT(DISTINCT a.PartyId) FROM B2CAssignments a LEFT JOIN B2CPayments p ON p.B2CAssignmentId = a.Id WHERE a.TotalValue > ISNULL(p.PaidAmount, 0)) AS ActiveB2CParties,
                (SELECT ISNULL(SUM(a.TotalValue - ISNULL(p.PaidAmount, 0)), 0) FROM B2CAssignments a LEFT JOIN B2CPayments p ON p.B2CAssignmentId = a.Id) AS B2CPending
            """,
            ct: cancellationToken) ?? new BusinessDashboardDto();

        dashboard.RawStockKg = inventory.RawStockKg;
        dashboard.TotalBoxStock = inventory.TotalBoxStock;
        dashboard.LowStockAlert = inventory.LowStockAlert;
        dashboard.TotalReceivable = payments.TotalReceivable;
        dashboard.PendingPayments = payments.TotalReceivable;
        dashboard.CollectedThisMonth = payments.TotalCollectedThisMonth;
        dashboard.TopPendingParties = payments.TopPendingParties;
        return ServiceOperationResult.CreateWithSuccess(dashboard);
    }

    public async Task<ServiceOperationResult<MonthlySummaryDto>> Handle(
        GetMonthlySummaryQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow.AddHours(5.5);
        var year = request.Year.GetValueOrDefault(now.Year);
        var month = request.Month.GetValueOrDefault(now.Month);
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1);
        var summary = await _dapper.QuerySingleAsync<MonthlySummaryDto>(
            """
            SELECT
                (SELECT ISNULL(SUM(TotalValue), 0) FROM B2BOrders WHERE OrderDate >= @From AND OrderDate < @To) AS B2BSales,
                (SELECT ISNULL(SUM(TotalValue), 0) FROM B2CAssignments WHERE AssignmentDate >= @From AND AssignmentDate < @To) AS B2CSales,
                (SELECT ISNULL(SUM(Amount), 0) FROM Payments WHERE PaymentDate >= @From AND PaymentDate < @To) AS TotalCollected,
                (SELECT ISNULL(SUM(QuantityKg), 0) FROM RawStockEntries WHERE EntryDate >= @From AND EntryDate < @To) AS RawStockPurchasedKg,
                (SELECT ISNULL(SUM(QuantityKg * CostPerKg), 0) FROM RawStockEntries WHERE EntryDate >= @From AND EntryDate < @To) AS RawStockPurchaseValue,
                (
                    SELECT ISNULL(SUM(Margin), 0) FROM B2BOrders WHERE OrderDate >= @From AND OrderDate < @To
                ) + (
                    SELECT ISNULL(SUM(Margin), 0) FROM B2CAssignments WHERE AssignmentDate >= @From AND AssignmentDate < @To
                ) AS TotalMargin
            """,
            new { From = from, To = to },
            ct: cancellationToken) ?? new MonthlySummaryDto();

        return ServiceOperationResult.CreateWithSuccess(summary);
    }

    public async Task<ServiceOperationResult<PurchaseReportDto>> Handle(
        GetPurchaseReportQuery request,
        CancellationToken cancellationToken)
    {
        var (from, to) = DateRange(request.From, request.To);
        var report = await _dapper.QueryMultipleAsync(
            """
            SELECT r.Id, r.EntryDate, r.SuttaGrade, COALESCE(s.Name, r.SupplierName, 'Unknown Seller') AS SellerName,
                   r.QuantityKg, r.CostPerKg, r.QuantityKg * r.CostPerKg AS TotalPayable, r.AvailableKg
            FROM RawStockEntries r
            LEFT JOIN Parties s ON s.Id = r.SellerPartyId
            WHERE r.EntryDate >= @From AND r.EntryDate < @To
            ORDER BY r.EntryDate DESC, r.Id DESC;

            SELECT SuttaGrade,
                   ISNULL(SUM(AvailableKg), 0) AS AvailableKg,
                   CASE WHEN ISNULL(SUM(AvailableKg), 0) > 0
                        THEN ISNULL(SUM(AvailableKg * CostPerKg), 0) / SUM(AvailableKg)
                        ELSE 0 END AS AverageCostPerKg,
                   ISNULL(SUM(AvailableKg * CostPerKg), 0) AS StockValue
            FROM RawStockEntries
            WHERE EntryDate >= @From AND EntryDate < @To
            GROUP BY SuttaGrade
            ORDER BY SuttaGrade;
            """,
            new { From = from, To = to },
            async reader =>
            {
                var rows = (await reader.ReadAsync<PurchaseReportRowDto>()).AsList();
                var grades = (await reader.ReadAsync<GradeStockSummaryDto>()).AsList();
                var totalQuantity = rows.Sum(x => x.QuantityKg);
                var totalPayable = rows.Sum(x => x.TotalPayable);
                return new PurchaseReportDto
                {
                    Rows = rows,
                    GradeSummary = grades,
                    TotalQuantityKg = totalQuantity,
                    TotalPayable = totalPayable,
                    AverageCostPerKg = totalQuantity > 0 ? Math.Round(totalPayable / totalQuantity, 2) : 0m
                };
            },
            ct: cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(report);
    }

    public async Task<ServiceOperationResult<SalesReportDto>> Handle(
        GetSalesReportQuery request,
        CancellationToken cancellationToken)
    {
        var salesType = request.SalesType.Trim().Equals("B2C", StringComparison.OrdinalIgnoreCase) ? "B2C" : "B2B";
        var (from, to) = DateRange(request.From, request.To);
        var sql = salesType == "B2C" ? B2CSalesReportSql : B2BSalesReportSql;
        var rows = await _dapper.QueryAsync<SalesReportRowDto>(
            sql,
            new { From = from, To = to },
            ct: cancellationToken,
            commandTimeoutSeconds: 30);

        return ServiceOperationResult.CreateWithSuccess(new SalesReportDto
        {
            SalesType = salesType,
            Rows = rows,
            TotalSales = rows.Sum(x => x.TotalValue),
            TotalCost = rows.Sum(x => x.TotalCost),
            TotalMargin = rows.Sum(x => x.Margin),
            TotalCollected = rows.Sum(x => x.PaidAmount),
            TotalPending = rows.Sum(x => x.PendingAmount)
        });
    }

    public async Task<ServiceOperationResult<ProfitLossReportDto>> Handle(
        GetProfitLossReportQuery request,
        CancellationToken cancellationToken)
    {
        var (from, to) = DateRange(request.From, request.To);
        var report = await _dapper.QueryMultipleAsync(
            """
            WITH B2BPayments AS (
                SELECT B2BOrderId, SUM(Amount) PaidAmount FROM Payments WHERE B2BOrderId IS NOT NULL GROUP BY B2BOrderId
            ),
            B2CPayments AS (
                SELECT B2CAssignmentId, SUM(Amount) PaidAmount FROM Payments WHERE B2CAssignmentId IS NOT NULL GROUP BY B2CAssignmentId
            )
            SELECT
                (SELECT ISNULL(SUM(TotalValue), 0) FROM B2BOrders WHERE OrderDate >= @From AND OrderDate < @To) AS B2BSales,
                (SELECT ISNULL(SUM(TotalValue), 0) FROM B2CAssignments WHERE AssignmentDate >= @From AND AssignmentDate < @To) AS B2CSales,
                (SELECT ISNULL(SUM(TotalCost), 0) FROM B2BOrders WHERE OrderDate >= @From AND OrderDate < @To)
                + (SELECT ISNULL(SUM(TotalCost), 0) FROM B2CAssignments WHERE AssignmentDate >= @From AND AssignmentDate < @To) AS PurchaseCostSold,
                (SELECT ISNULL(SUM(TotalValue - ISNULL(p.PaidAmount, 0)), 0)
                 FROM B2BOrders o LEFT JOIN B2BPayments p ON p.B2BOrderId = o.Id)
                + (SELECT ISNULL(SUM(TotalValue - ISNULL(p.PaidAmount, 0)), 0)
                 FROM B2CAssignments a LEFT JOIN B2CPayments p ON p.B2CAssignmentId = a.Id) AS PendingReceivable;

            SELECT Label, SUM(Sales) AS Sales, SUM(Cost) AS Cost, SUM(Profit) AS Profit
            FROM (
                SELECT SuttaGrade AS Label, TotalValue AS Sales, TotalCost AS Cost, Margin AS Profit
                FROM B2BOrders
                WHERE OrderDate >= @From AND OrderDate < @To
                UNION ALL
                SELECT pr.Name AS Label, a.TotalValue AS Sales, a.TotalCost AS Cost, a.Margin AS Profit
                FROM B2CAssignments a
                INNER JOIN Products pr ON pr.Id = a.ProductId
                WHERE a.AssignmentDate >= @From AND a.AssignmentDate < @To
            ) grouped
            GROUP BY Label
            ORDER BY SUM(Profit) DESC;
            """,
            new { From = from, To = to },
            async reader =>
            {
                var report = await reader.ReadFirstAsync<ProfitLossReportDto>();
                var grades = (await reader.ReadAsync<ProfitByGradeDto>()).AsList();
                report.TotalSales = report.B2BSales + report.B2CSales;
                report.GrossProfit = report.TotalSales - report.PurchaseCostSold;
                report.GrossProfitPercent = report.TotalSales > 0 ? Math.Round(report.GrossProfit / report.TotalSales * 100m, 2) : 0m;
                report.GradeProfit = grades;
                return report;
            },
            ct: cancellationToken,
            commandTimeoutSeconds: 30);

        return ServiceOperationResult.CreateWithSuccess(report);
    }

    public async Task<ServiceOperationResult<StockValuationReportDto>> Handle(
        GetStockValuationReportQuery request,
        CancellationToken cancellationToken)
    {
        var inventory = await BuildInventorySummaryAsync(cancellationToken);
        var boxValue = inventory.BoxStock.Sum(x => x.AvailableQuantity * x.AverageUnitCost);
        var report = new StockValuationReportDto
        {
            RawStockKg = inventory.RawStockKg,
            RawStockValue = inventory.RawStockValue,
            BoxStockQuantity = inventory.TotalBoxStock,
            BoxStockValue = Math.Round(boxValue, 2),
            TotalStockValue = Math.Round(inventory.RawStockValue + boxValue, 2),
            GradeStock = inventory.GradeStock,
            BoxStock = inventory.BoxStock
        };

        return ServiceOperationResult.CreateWithSuccess(report);
    }

    private const string B2BSalesReportSql = """
        WITH PaymentTotals AS (
            SELECT B2BOrderId, SUM(Amount) AS PaidAmount
            FROM Payments
            WHERE B2BOrderId IS NOT NULL
            GROUP BY B2BOrderId
        )
        SELECT o.Id, 'B2B' AS SalesType, o.OrderDate AS SalesDate, p.Name AS PartyName,
               o.SuttaGrade AS ItemLabel, o.QuantityKg AS Quantity, o.PricePerKg AS Rate,
               o.TotalValue, o.TotalCost, o.Margin,
               ISNULL(pt.PaidAmount, 0) AS PaidAmount,
               o.TotalValue - ISNULL(pt.PaidAmount, 0) AS PendingAmount,
               i.InvoiceNumber
        FROM B2BOrders o
        INNER JOIN Parties p ON p.Id = o.PartyId
        LEFT JOIN PaymentTotals pt ON pt.B2BOrderId = o.Id
        LEFT JOIN Invoices i ON i.B2BOrderId = o.Id
        WHERE o.OrderDate >= @From AND o.OrderDate < @To
        ORDER BY o.OrderDate DESC, o.Id DESC;
        """;

    private const string B2CSalesReportSql = """
        WITH PaymentTotals AS (
            SELECT B2CAssignmentId, SUM(Amount) AS PaidAmount
            FROM Payments
            WHERE B2CAssignmentId IS NOT NULL
            GROUP BY B2CAssignmentId
        )
        SELECT a.Id, 'B2C' AS SalesType, a.AssignmentDate AS SalesDate, p.Name AS PartyName,
               pr.Name AS ItemLabel, CAST(a.Quantity AS decimal(18, 2)) AS Quantity, a.UnitPrice AS Rate,
               a.TotalValue, a.TotalCost, a.Margin,
               ISNULL(pt.PaidAmount, 0) AS PaidAmount,
               a.TotalValue - ISNULL(pt.PaidAmount, 0) AS PendingAmount,
               i.InvoiceNumber
        FROM B2CAssignments a
        INNER JOIN Parties p ON p.Id = a.PartyId
        INNER JOIN Products pr ON pr.Id = a.ProductId
        LEFT JOIN PaymentTotals pt ON pt.B2CAssignmentId = a.Id
        LEFT JOIN Invoices i ON i.B2CAssignmentId = a.Id
        WHERE a.AssignmentDate >= @From AND a.AssignmentDate < @To
        ORDER BY a.AssignmentDate DESC, a.Id DESC;
        """;

    private static (DateTime From, DateTime To) DateRange(DateTime? from, DateTime? to)
    {
        var now = DateTime.UtcNow.AddHours(5.5);
        var rangeFrom = from?.Date ?? new DateTime(now.Year, now.Month, 1);
        var rangeTo = to?.Date.AddDays(1) ?? now.Date.AddDays(1);
        return (rangeFrom, rangeTo);
    }

    private async Task<InventorySummaryDto> BuildInventorySummaryAsync(CancellationToken cancellationToken)
    {
        var result = await _dapper.QueryMultipleAsync(
            """
            SELECT ISNULL(SUM(AvailableKg), 0) AS RawStockKg,
                   ISNULL(SUM(AvailableKg * CostPerKg), 0) AS RawStockValue
            FROM RawStockEntries;

            WITH SignedMovements AS (
                SELECT ProductId,
                       CASE WHEN MovementType = 'ASSIGNED' THEN -Quantity ELSE Quantity END AS SignedQuantity,
                       CASE WHEN MovementType = 'ASSIGNED' THEN -Quantity * UnitCost ELSE Quantity * UnitCost END AS SignedValue
                FROM BoxStockMovements
            )
            SELECT p.Id AS ProductId, p.Name AS ProductName, p.Weight, p.WeightGrams,
                   ISNULL(SUM(sm.SignedQuantity), 0) AS AvailableQuantity,
                   CASE WHEN ISNULL(SUM(sm.SignedQuantity), 0) > 0
                        THEN ISNULL(SUM(sm.SignedValue), 0) / SUM(sm.SignedQuantity)
                        ELSE 0 END AS AverageUnitCost,
                   p.CurrentPrice
            FROM Products p
            LEFT JOIN SignedMovements sm ON sm.ProductId = p.Id
            GROUP BY p.Id, p.Name, p.Weight, p.WeightGrams, p.CurrentPrice
            ORDER BY p.Name;

            SELECT SuttaGrade,
                   ISNULL(SUM(AvailableKg), 0) AS AvailableKg,
                   CASE WHEN ISNULL(SUM(AvailableKg), 0) > 0
                        THEN ISNULL(SUM(AvailableKg * CostPerKg), 0) / SUM(AvailableKg)
                        ELSE 0 END AS AverageCostPerKg,
                   ISNULL(SUM(AvailableKg * CostPerKg), 0) AS StockValue
            FROM RawStockEntries
            WHERE AvailableKg > 0
            GROUP BY SuttaGrade
            ORDER BY SuttaGrade;
            """,
            null,
            async reader =>
            {
                var raw = await reader.ReadFirstAsync<InventorySummaryDto>();
                var boxes = (await reader.ReadAsync<BoxStockSummaryDto>()).AsList();
                var grades = (await reader.ReadAsync<GradeStockSummaryDto>()).AsList();
                raw.BoxStock = boxes;
                raw.GradeStock = grades;
                raw.TotalBoxStock = boxes.Sum(x => x.AvailableQuantity);
                raw.LowStockAlert = raw.RawStockKg < 25 || boxes.Any(x => x.AvailableQuantity < 10);
                return raw;
            },
            ct: cancellationToken);

        return result;
    }

    private async Task<PaymentSummaryDto> BuildPaymentSummaryAsync(CancellationToken cancellationToken)
    {
        var result = await _dapper.QueryMultipleAsync(
            """
            WITH B2BPending AS (
                SELECT o.PartyId, o.DueDate, o.TotalValue - ISNULL(SUM(pay.Amount), 0) AS PendingAmount
                FROM B2BOrders o
                LEFT JOIN Payments pay ON pay.B2BOrderId = o.Id
                GROUP BY o.Id, o.PartyId, o.DueDate, o.TotalValue
            ),
            B2CPending AS (
                SELECT a.PartyId, a.DueDate, a.TotalValue - ISNULL(SUM(pay.Amount), 0) AS PendingAmount
                FROM B2CAssignments a
                LEFT JOIN Payments pay ON pay.B2CAssignmentId = a.Id
                GROUP BY a.Id, a.PartyId, a.DueDate, a.TotalValue
            ),
            Pending AS (
                SELECT PartyId, DueDate, PendingAmount FROM B2BPending WHERE PendingAmount > 0
                UNION ALL
                SELECT PartyId, DueDate, PendingAmount FROM B2CPending WHERE PendingAmount > 0
            )
            SELECT ISNULL(SUM(PendingAmount), 0) AS TotalReceivable,
                   (SELECT ISNULL(SUM(Amount), 0) FROM Payments WHERE PaymentDate >= @MonthStart AND PaymentDate < @NextMonthStart) AS TotalCollectedThisMonth
            FROM Pending;

            WITH B2BPending AS (
                SELECT o.PartyId, o.DueDate, o.TotalValue - ISNULL(SUM(pay.Amount), 0) AS PendingAmount
                FROM B2BOrders o
                LEFT JOIN Payments pay ON pay.B2BOrderId = o.Id
                GROUP BY o.Id, o.PartyId, o.DueDate, o.TotalValue
            ),
            B2CPending AS (
                SELECT a.PartyId, a.DueDate, a.TotalValue - ISNULL(SUM(pay.Amount), 0) AS PendingAmount
                FROM B2CAssignments a
                LEFT JOIN Payments pay ON pay.B2CAssignmentId = a.Id
                GROUP BY a.Id, a.PartyId, a.DueDate, a.TotalValue
            ),
            Pending AS (
                SELECT PartyId, DueDate, PendingAmount FROM B2BPending WHERE PendingAmount > 0
                UNION ALL
                SELECT PartyId, DueDate, PendingAmount FROM B2CPending WHERE PendingAmount > 0
            )
            SELECT TOP 10 p.Id AS PartyId, p.Name AS PartyName, p.PartyType,
                   SUM(pe.PendingAmount) AS PendingAmount,
                   MIN(pe.DueDate) AS OldestDueDate,
                   CAST(CASE WHEN MIN(CAST(pe.DueDate AS date)) < @Today THEN 1 ELSE 0 END AS bit) AS IsOverdue
            FROM Pending pe
            INNER JOIN Parties p ON p.Id = pe.PartyId
            GROUP BY p.Id, p.Name, p.PartyType
            ORDER BY SUM(pe.PendingAmount) DESC;
            """,
            new
            {
                MonthStart = MonthStartInIndia(),
                NextMonthStart = MonthStartInIndia().AddMonths(1),
                Today = TodayInIndia()
            },
            async reader =>
            {
                var summary = await reader.ReadFirstAsync<PaymentSummaryDto>();
                summary.TopPendingParties = (await reader.ReadAsync<PendingPartyDto>()).AsList();
                return summary;
            },
            ct: cancellationToken);

        return result;
    }

    private static DateTime TodayInIndia()
        => DateTime.UtcNow.AddHours(5.5).Date;

    private static DateTime MonthStartInIndia()
    {
        var now = DateTime.UtcNow.AddHours(5.5);
        return new DateTime(now.Year, now.Month, 1);
    }
}
