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
}
