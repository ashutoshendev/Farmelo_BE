namespace Farmelo.Shared.OperationResult;

public sealed class ExcelResponse
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string Base64Content { get; set; } = string.Empty;
}
