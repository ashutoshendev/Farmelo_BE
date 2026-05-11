namespace Farmelo.API.Middleware;

public sealed class DecodeUrlMiddleware
{
    private readonly RequestDelegate _next;

    public DecodeUrlMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext.Request.QueryString.HasValue &&
            !string.IsNullOrEmpty(httpContext.Request.QueryString.Value))
        {
            var decodedUrlString = DecodeUrlString(httpContext.Request.QueryString.Value[1..]);
            httpContext.Request.QueryString = new QueryString($"?{decodedUrlString}");
        }

        await _next(httpContext);
    }

    private static string DecodeUrlString(string url)
    {
        string newUrl;
        while ((newUrl = Uri.UnescapeDataString(url)) != url)
        {
            url = newUrl;
        }

        return newUrl;
    }
}
