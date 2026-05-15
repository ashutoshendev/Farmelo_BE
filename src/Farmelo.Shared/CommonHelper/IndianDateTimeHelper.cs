namespace Farmelo.Shared.CommonHelper;

public static class IndianDateTimeHelper
{
    private static readonly TimeZoneInfo IndianTimeZone = ResolveIndianTimeZone();

    public static DateTime ToUtcStartOfIndianDay(DateTime value)
    {
        var indianDate = DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(indianDate, IndianTimeZone);
    }

    public static DateTime ToUtcEndOfIndianDay(DateTime value)
    {
        var nextIndianDate = DateTime.SpecifyKind(value.Date.AddDays(1), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(nextIndianDate, IndianTimeZone).AddTicks(-1);
    }

    private static TimeZoneInfo ResolveIndianTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        }
    }
}
