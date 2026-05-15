using Farmelo.Shared.DTO.Business;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Queries.Business;

public sealed record GetPartiesQuery(string? PartyType)
    : IRequest<ServiceOperationResult<IReadOnlyList<PartyDto>>>;

public sealed record GetRawStockEntriesQuery
    : IRequest<ServiceOperationResult<IReadOnlyList<RawStockEntryDto>>>;

public sealed record GetInventorySummaryQuery
    : IRequest<ServiceOperationResult<InventorySummaryDto>>;

public sealed record GetB2BOrdersQuery(int? PartyId)
    : IRequest<ServiceOperationResult<IReadOnlyList<B2BOrderDto>>>;

public sealed record GetB2CAssignmentsQuery(int? PartyId)
    : IRequest<ServiceOperationResult<IReadOnlyList<B2CAssignmentDto>>>;

public sealed record GetPaymentsQuery(int? PartyId)
    : IRequest<ServiceOperationResult<IReadOnlyList<PaymentDto>>>;

public sealed record GetPaymentSummaryQuery
    : IRequest<ServiceOperationResult<PaymentSummaryDto>>;

public sealed record GetBusinessDashboardQuery
    : IRequest<ServiceOperationResult<BusinessDashboardDto>>;

public sealed record GetMonthlySummaryQuery(int? Year, int? Month)
    : IRequest<ServiceOperationResult<MonthlySummaryDto>>;

public sealed record GetPurchaseReportQuery(DateTime? From, DateTime? To)
    : IRequest<ServiceOperationResult<PurchaseReportDto>>;

public sealed record GetSalesReportQuery(string SalesType, DateTime? From, DateTime? To)
    : IRequest<ServiceOperationResult<SalesReportDto>>;

public sealed record GetProfitLossReportQuery(DateTime? From, DateTime? To)
    : IRequest<ServiceOperationResult<ProfitLossReportDto>>;

public sealed record GetStockValuationReportQuery
    : IRequest<ServiceOperationResult<StockValuationReportDto>>;
