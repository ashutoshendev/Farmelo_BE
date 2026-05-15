namespace Farmelo.Shared.DTO.Dashboard;

public sealed class AdminDashboardSummaryDto
{
    public int TotalOwners { get; set; }
    public int ActiveOwners { get; set; }
    public int ActiveProducts { get; set; }
    public int RecentActions { get; set; }
}
