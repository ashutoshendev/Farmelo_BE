namespace Farmelo.Shared.DTO.Business;

public sealed class PartyDto
{
    public int Id { get; set; }
    public string PartyType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public sealed class PartyWriteRequestDto
{
    public string PartyType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class RawStockEntryDto
{
    public int Id { get; set; }
    public int? SellerPartyId { get; set; }
    public string? SellerName { get; set; }
    public DateTime EntryDate { get; set; }
    public string SuttaGrade { get; set; } = string.Empty;
    public decimal QuantityKg { get; set; }
    public decimal AvailableKg { get; set; }
    public decimal CostPerKg { get; set; }
    public decimal TotalCost { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }
}

public sealed class RawStockEntryCreateRequestDto
{
    public int? SellerPartyId { get; set; }
    public DateTime? EntryDate { get; set; }
    public string SuttaGrade { get; set; } = string.Empty;
    public decimal QuantityKg { get; set; }
    public decimal CostPerKg { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }
}

public sealed class BoxProductionCreateRequestDto
{
    public int ProductId { get; set; }
    public string SuttaGrade { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime? ProductionDate { get; set; }
    public string? Notes { get; set; }
}

public sealed class BoxStockSummaryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public int WeightGrams { get; set; }
    public int AvailableQuantity { get; set; }
    public decimal AverageUnitCost { get; set; }
    public decimal CurrentPrice { get; set; }
}

public sealed class InventorySummaryDto
{
    public decimal RawStockKg { get; set; }
    public decimal RawStockValue { get; set; }
    public int TotalBoxStock { get; set; }
    public bool LowStockAlert { get; set; }
    public IReadOnlyList<BoxStockSummaryDto> BoxStock { get; set; } = Array.Empty<BoxStockSummaryDto>();
    public IReadOnlyList<GradeStockSummaryDto> GradeStock { get; set; } = Array.Empty<GradeStockSummaryDto>();
}

public sealed class B2BOrderCreateRequestDto
{
    public int PartyId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string SuttaGrade { get; set; } = string.Empty;
    public decimal QuantityKg { get; set; }
    public decimal PricePerKg { get; set; }
    public string? Notes { get; set; }
}

public sealed class B2BOrderDto
{
    public int Id { get; set; }
    public int PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime DueDate { get; set; }
    public string SuttaGrade { get; set; } = string.Empty;
    public decimal QuantityKg { get; set; }
    public decimal PricePerKg { get; set; }
    public decimal TotalValue { get; set; }
    public decimal CostPerKg { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Margin { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public bool IsOverdue { get; set; }
    public string? Notes { get; set; }
    public long? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? InvoiceEmailStatus { get; set; }
    public string? InvoiceDownloadUrl { get; set; }
    public InvoiceDto? Invoice { get; set; }
}

public sealed class B2CAssignmentCreateRequestDto
{
    public int PartyId { get; set; }
    public int ProductId { get; set; }
    public DateTime? AssignmentDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Notes { get; set; }
}

public sealed class B2CReturnRequestDto
{
    public int AssignmentId { get; set; }
    public int Quantity { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? Notes { get; set; }
}

public sealed class B2CAssignmentDto
{
    public int Id { get; set; }
    public int PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public DateTime AssignmentDate { get; set; }
    public DateTime DueDate { get; set; }
    public int Quantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Margin { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public bool IsOverdue { get; set; }
    public string? Notes { get; set; }
    public long? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? InvoiceEmailStatus { get; set; }
    public string? InvoiceDownloadUrl { get; set; }
    public InvoiceDto? Invoice { get; set; }
}

public sealed class PaymentCreateRequestDto
{
    public string PaymentType { get; set; } = string.Empty;
    public int? B2BOrderId { get; set; }
    public int? B2CAssignmentId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMode { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public sealed class PaymentDto
{
    public int Id { get; set; }
    public int PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public int? B2BOrderId { get; set; }
    public int? B2CAssignmentId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMode { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public sealed class PendingPartyDto
{
    public int PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string PartyType { get; set; } = string.Empty;
    public decimal PendingAmount { get; set; }
    public DateTime OldestDueDate { get; set; }
    public bool IsOverdue { get; set; }
}

public sealed class PaymentSummaryDto
{
    public decimal TotalReceivable { get; set; }
    public decimal TotalCollectedThisMonth { get; set; }
    public IReadOnlyList<PendingPartyDto> TopPendingParties { get; set; } = Array.Empty<PendingPartyDto>();
}

public sealed class BusinessDashboardDto
{
    public decimal RawStockKg { get; set; }
    public int TotalBoxStock { get; set; }
    public decimal TotalReceivable { get; set; }
    public decimal CollectedThisMonth { get; set; }
    public decimal PendingPayments { get; set; }
    public int ActiveB2BOrders { get; set; }
    public decimal B2BPending { get; set; }
    public int ActiveB2CParties { get; set; }
    public decimal B2CPending { get; set; }
    public bool LowStockAlert { get; set; }
    public IReadOnlyList<PendingPartyDto> TopPendingParties { get; set; } = Array.Empty<PendingPartyDto>();
}

public sealed class MonthlySummaryDto
{
    public decimal B2BSales { get; set; }
    public decimal B2CSales { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal RawStockPurchasedKg { get; set; }
    public decimal RawStockPurchaseValue { get; set; }
    public decimal TotalMargin { get; set; }
}

public sealed class GradeStockSummaryDto
{
    public string SuttaGrade { get; set; } = string.Empty;
    public decimal AvailableKg { get; set; }
    public decimal AverageCostPerKg { get; set; }
    public decimal StockValue { get; set; }
}

public sealed class InvoiceDto
{
    public long Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceType { get; set; } = string.Empty;
    public int PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string EmailStatus { get; set; } = string.Empty;
    public string? EmailError { get; set; }
    public DateTime? EmailSentOn { get; set; }
    public string? DownloadUrl { get; set; }
}

public sealed class ReportDateRangeRequestDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public sealed class PurchaseReportRowDto
{
    public int Id { get; set; }
    public DateTime EntryDate { get; set; }
    public string SuttaGrade { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public decimal QuantityKg { get; set; }
    public decimal CostPerKg { get; set; }
    public decimal TotalPayable { get; set; }
    public decimal AvailableKg { get; set; }
}

public sealed class PurchaseReportDto
{
    public decimal TotalQuantityKg { get; set; }
    public decimal TotalPayable { get; set; }
    public decimal AverageCostPerKg { get; set; }
    public IReadOnlyList<PurchaseReportRowDto> Rows { get; set; } = Array.Empty<PurchaseReportRowDto>();
    public IReadOnlyList<GradeStockSummaryDto> GradeSummary { get; set; } = Array.Empty<GradeStockSummaryDto>();
}

public sealed class SalesReportRowDto
{
    public int Id { get; set; }
    public string SalesType { get; set; } = string.Empty;
    public DateTime SalesDate { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string ItemLabel { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Margin { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public string? InvoiceNumber { get; set; }
}

public sealed class SalesReportDto
{
    public string SalesType { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalMargin { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalPending { get; set; }
    public IReadOnlyList<SalesReportRowDto> Rows { get; set; } = Array.Empty<SalesReportRowDto>();
}

public sealed class ProfitLossReportDto
{
    public decimal B2BSales { get; set; }
    public decimal B2CSales { get; set; }
    public decimal TotalSales { get; set; }
    public decimal PurchaseCostSold { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal GrossProfitPercent { get; set; }
    public decimal PendingReceivable { get; set; }
    public IReadOnlyList<ProfitByGradeDto> GradeProfit { get; set; } = Array.Empty<ProfitByGradeDto>();
}

public sealed class ProfitByGradeDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Sales { get; set; }
    public decimal Cost { get; set; }
    public decimal Profit { get; set; }
}

public sealed class StockValuationReportDto
{
    public decimal RawStockKg { get; set; }
    public decimal RawStockValue { get; set; }
    public int BoxStockQuantity { get; set; }
    public decimal BoxStockValue { get; set; }
    public decimal TotalStockValue { get; set; }
    public IReadOnlyList<GradeStockSummaryDto> GradeStock { get; set; } = Array.Empty<GradeStockSummaryDto>();
    public IReadOnlyList<BoxStockSummaryDto> BoxStock { get; set; } = Array.Empty<BoxStockSummaryDto>();
}
