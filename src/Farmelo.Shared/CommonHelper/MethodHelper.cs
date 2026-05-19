using Newtonsoft.Json;
using System.ComponentModel;
using System.IO.Compression;
using System.Text;

namespace Farmelo.Shared.CommonHelper;

public static class MethodHelper
{
    public static string FormatException(Exception ex, string errorSource = "Farmelo API", string correlationId = "")
    {
        return ex == null
            ? string.Empty
            : $"|ErrorSource: {errorSource} | CorrelationId: {correlationId} |Message: {ex.Message}|Exception: {ex}|Stack Trace: {ex.StackTrace}";
    }

    public static T? Convert<T>(this string input)
    {
        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return converter == null ? default : (T?)converter.ConvertFromString(input);
        }
        catch (NotSupportedException)
        {
            return default;
        }
    }

    public static string FromObjectToJsonString<T>(T fromObject)
        => JsonConvert.SerializeObject(fromObject);

    public static T? FromJsonStringToObject<T>(string fromString)
        => string.IsNullOrEmpty(fromString) ? default : JsonConvert.DeserializeObject<T>(fromString);

    public static string Zip(string stringToBeZipped)
    {
        var bytes = Encoding.UTF8.GetBytes(stringToBeZipped);

        using var source = new MemoryStream(bytes);
        using var target = new MemoryStream();
        using (var gzip = new GZipStream(target, CompressionMode.Compress))
        {
            source.CopyTo(gzip);
        }

        return System.Convert.ToBase64String(target.ToArray());
    }

    public static string Unzip(string stringToBeUnZipped)
    {
        if (string.IsNullOrWhiteSpace(stringToBeUnZipped))
        {
            return string.Empty;
        }

        try
        {
            var bytes = System.Convert.FromBase64String(stringToBeUnZipped);
            using var source = new MemoryStream(bytes);
            using var target = new MemoryStream();
            using (var gzip = new GZipStream(source, CompressionMode.Decompress))
            {
                gzip.CopyTo(target);
            }

            return Encoding.UTF8.GetString(target.ToArray());
        }
        catch (FormatException)
        {
            return stringToBeUnZipped;
        }
        catch (InvalidDataException)
        {
            return stringToBeUnZipped;
        }
        catch (IOException)
        {
            return stringToBeUnZipped;
        }
    }
}
