namespace Farmelo.Business.Support;

internal static class TextNormalizer
{
    public static string NormalizeEmail(string email)
        => email.Trim().ToLowerInvariant();

    public static string NormalizeSlug(string slug)
        => slug.Trim().ToLowerInvariant();

    public static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string CurrentUserNameOrSystem(string? userName)
        => string.IsNullOrWhiteSpace(userName) ? "SYSTEM" : userName.Trim();
}
