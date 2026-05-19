namespace Farmelo.Shared.Validators;

internal static class ValidationPatterns
{
    public const string Slug = "^[a-z0-9-]+$";
    public const string CurrencyCode = "^[A-Za-z]{3}$";
    public const string HexColor = "^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$";
    public const string StrongPassword = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d]).+$";
}
