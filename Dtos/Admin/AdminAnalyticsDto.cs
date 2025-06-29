namespace backend.Dtos.Admin
{
    public class AdminAnalyticsDto
    {
        public List<UserGrowthDto> UserGrowth { get; set; } = new List<UserGrowthDto>();
        public List<RoleDistributionDto> RoleDistribution { get; set; } = new List<RoleDistributionDto>();
        public List<SectionStatsDto> SectionStats { get; set; } = new List<SectionStatsDto>();
        public List<RecentActivityDto> RecentActivity { get; set; } = new List<RecentActivityDto>();
    }

    public class UserGrowthDto
    {
        public string Month { get; set; } = string.Empty;
        public int Users { get; set; }
    }

    public class RoleDistributionDto
    {
        public string Role { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class SectionStatsDto
    {
        public int SectionId { get; set; }
        public int StudentCount { get; set; }
        public string Instructor { get; set; } = string.Empty;
    }

    public class RecentActivityDto
    {
        public string Action { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Trend { get; set; } = string.Empty; // "up", "down", "stable"
    }
} 