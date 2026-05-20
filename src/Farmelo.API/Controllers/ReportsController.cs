using ClosedXML.Excel;
using Farmelo.API.Controllers.Abstractions;
using Farmelo.Business.Queries.Business;
using Farmelo.Shared.CommonHelper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmelo.API.Controllers;

[Authorize(Roles = AppConstants.Roles.Admin + "," + AppConstants.Roles.Owner)]
[Route("api/reports")]
public sealed class ReportsController : ApiBaseController<ReportsController>
{
    public ReportsController(ILogger<ReportsController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    [HttpGet("monthly-summary")]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
        => Ok(await Mediator.Send(new GetMonthlySummaryQuery(year, month), ct));

    [HttpGet("purchase")]
    public async Task<IActionResult> GetPurchaseReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await Mediator.Send(new GetPurchaseReportQuery(from, to), ct));

    [HttpGet("sales/b2b")]
    public async Task<IActionResult> GetB2BSalesReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await Mediator.Send(new GetSalesReportQuery("B2B", from, to), ct));

    [HttpGet("sales/b2c")]
    public async Task<IActionResult> GetB2CSalesReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await Mediator.Send(new GetSalesReportQuery("B2C", from, to), ct));

    [HttpGet("profit-loss")]
    public async Task<IActionResult> GetProfitLossReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await Mediator.Send(new GetProfitLossReportQuery(from, to), ct));

    [HttpGet("stock-valuation")]
    public async Task<IActionResult> GetStockValuationReport(CancellationToken ct)
        => Ok(await Mediator.Send(new GetStockValuationReportQuery(), ct));

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMonthlySummaryQuery(year, month), ct);
        if (!result.Success || result.Payload == null)
        {
            return BadRequest(result);
        }

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Monthly Summary");
        sheet.Cell(1, 1).Value = "Metric";
        sheet.Cell(1, 2).Value = "Value";
        sheet.Cell(2, 1).Value = "B2B Sales";
        sheet.Cell(2, 2).Value = result.Payload.B2BSales;
        sheet.Cell(3, 1).Value = "B2C Sales";
        sheet.Cell(3, 2).Value = result.Payload.B2CSales;
        sheet.Cell(4, 1).Value = "Collected";
        sheet.Cell(4, 2).Value = result.Payload.TotalCollected;
        sheet.Cell(5, 1).Value = "Raw Stock KG";
        sheet.Cell(5, 2).Value = result.Payload.RawStockPurchasedKg;
        sheet.Cell(6, 1).Value = "Raw Stock Purchase Value";
        sheet.Cell(6, 2).Value = result.Payload.RawStockPurchaseValue;
        sheet.Cell(7, 1).Value = "Margin";
        sheet.Cell(7, 2).Value = result.Payload.TotalMargin;
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "farmelo-monthly-summary.xlsx");
    }

    [HttpGet("purchase/export/excel")]
    public async Task<IActionResult> ExportPurchaseExcel([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPurchaseReportQuery(from, to), ct);
        if (!result.Success || result.Payload == null)
        {
            return BadRequest(result);
        }

        using var workbook = new XLWorkbook();
        var summary = workbook.Worksheets.Add("Summary");
        WriteRows(summary, new[]
        {
            new object[] { "Metric", "Value" },
            new object[] { "Total KG Bought", result.Payload.TotalQuantityKg },
            new object[] { "Seller Payable", result.Payload.TotalPayable },
            new object[] { "Average Cost/KG", result.Payload.AverageCostPerKg },
        });

        var rows = workbook.Worksheets.Add("Rows");
        WriteRows(rows, new[]
        {
            new object[] { "Date", "Seller", "Grade", "Quantity KG", "Cost/KG", "Payable", "Available KG" },
        }.Concat(result.Payload.Rows.Select(row => new object[]
        {
            row.EntryDate,
            row.SellerName,
            row.SuttaGrade,
            row.QuantityKg,
            row.CostPerKg,
            row.TotalPayable,
            row.AvailableKg,
        })));

        return ExcelFile(workbook, "farmelo-purchase-report.xlsx");
    }

    [HttpGet("sales/{type}/export/excel")]
    public async Task<IActionResult> ExportSalesExcel([FromRoute] string type, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var normalizedType = string.Equals(type, "b2c", StringComparison.OrdinalIgnoreCase) ? "B2C" : "B2B";
        var result = await Mediator.Send(new GetSalesReportQuery(normalizedType, from, to), ct);
        if (!result.Success || result.Payload == null)
        {
            return BadRequest(result);
        }

        using var workbook = new XLWorkbook();
        var summary = workbook.Worksheets.Add("Summary");
        WriteRows(summary, new[]
        {
            new object[] { "Metric", "Value" },
            new object[] { "Total Sales", result.Payload.TotalSales },
            new object[] { "Total Cost", result.Payload.TotalCost },
            new object[] { "Profit", result.Payload.TotalMargin },
            new object[] { "Collected", result.Payload.TotalCollected },
            new object[] { "Pending", result.Payload.TotalPending },
        });

        var rows = workbook.Worksheets.Add("Rows");
        WriteRows(rows, new[]
        {
            new object[] { "Date", "Party", "Item", "Quantity", "Rate", "Sales", "Cost", "Profit", "Paid", "Pending", "Invoice" },
        }.Concat(result.Payload.Rows.Select(row => new object[]
        {
            row.SalesDate,
            row.PartyName,
            row.ItemLabel,
            row.Quantity,
            row.Rate,
            row.TotalValue,
            row.TotalCost,
            row.Margin,
            row.PaidAmount,
            row.PendingAmount,
            row.InvoiceNumber ?? "Pending",
        })));

        return ExcelFile(workbook, $"farmelo-{normalizedType.ToLowerInvariant()}-sales-report.xlsx");
    }

    [HttpGet("profit-loss/export/excel")]
    public async Task<IActionResult> ExportProfitLossExcel([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetProfitLossReportQuery(from, to), ct);
        if (!result.Success || result.Payload == null)
        {
            return BadRequest(result);
        }

        using var workbook = new XLWorkbook();
        var summary = workbook.Worksheets.Add("Summary");
        WriteRows(summary, new[]
        {
            new object[] { "Metric", "Value" },
            new object[] { "B2B Sales", result.Payload.B2BSales },
            new object[] { "B2C Sales", result.Payload.B2CSales },
            new object[] { "Total Sales", result.Payload.TotalSales },
            new object[] { "Cost Sold", result.Payload.PurchaseCostSold },
            new object[] { "Gross Profit", result.Payload.GrossProfit },
            new object[] { "Profit %", result.Payload.GrossProfitPercent },
            new object[] { "Pending Receivable", result.Payload.PendingReceivable },
        });

        var rows = workbook.Worksheets.Add("Grade Profit");
        WriteRows(rows, new[]
        {
            new object[] { "Label", "Sales", "Cost", "Profit" },
        }.Concat(result.Payload.GradeProfit.Select(row => new object[]
        {
            row.Label,
            row.Sales,
            row.Cost,
            row.Profit,
        })));

        return ExcelFile(workbook, "farmelo-profit-loss-report.xlsx");
    }

    [HttpGet("stock-valuation/export/excel")]
    public async Task<IActionResult> ExportStockValuationExcel(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetStockValuationReportQuery(), ct);
        if (!result.Success || result.Payload == null)
        {
            return BadRequest(result);
        }

        using var workbook = new XLWorkbook();
        var summary = workbook.Worksheets.Add("Summary");
        WriteRows(summary, new[]
        {
            new object[] { "Metric", "Value" },
            new object[] { "Raw Stock KG", result.Payload.RawStockKg },
            new object[] { "Raw Stock Value", result.Payload.RawStockValue },
            new object[] { "Box Stock Quantity", result.Payload.BoxStockQuantity },
            new object[] { "Box Stock Value", result.Payload.BoxStockValue },
            new object[] { "Total Stock Value", result.Payload.TotalStockValue },
        });

        var gradeStock = workbook.Worksheets.Add("Grade Stock");
        WriteRows(gradeStock, new[]
        {
            new object[] { "Grade", "Available KG", "Average Cost/KG", "Value" },
        }.Concat(result.Payload.GradeStock.Select(row => new object[]
        {
            row.SuttaGrade,
            row.AvailableKg,
            row.AverageCostPerKg,
            row.StockValue,
        })));

        var boxStock = workbook.Worksheets.Add("Box Stock");
        WriteRows(boxStock, new[]
        {
            new object[] { "Product", "Weight", "Available Quantity", "Average Unit Cost", "Current Price" },
        }.Concat(result.Payload.BoxStock.Select(row => new object[]
        {
            row.ProductName,
            row.Weight,
            row.AvailableQuantity,
            row.AverageUnitCost,
            row.CurrentPrice,
        })));

        return ExcelFile(workbook, "farmelo-stock-valuation-report.xlsx");
    }

    private FileContentResult ExcelFile(XLWorkbook workbook, string fileName)
    {
        foreach (var sheet in workbook.Worksheets)
        {
            sheet.Columns().AdjustToContents();
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private static void WriteRows(IXLWorksheet sheet, IEnumerable<object[]> rows)
    {
        var rowIndex = 1;
        foreach (var row in rows)
        {
            for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
            {
                var cell = sheet.Cell(rowIndex, columnIndex + 1);
                cell.Value = row[columnIndex] switch
                {
                    null => string.Empty,
                    DateTime value => value,
                    decimal value => value,
                    double value => value,
                    float value => value,
                    int value => value,
                    long value => value,
                    bool value => value,
                    _ => row[columnIndex].ToString() ?? string.Empty,
                };
            }
            rowIndex++;
        }
    }
}
