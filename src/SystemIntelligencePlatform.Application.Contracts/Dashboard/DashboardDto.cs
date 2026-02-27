using System;
using System.Collections.Generic;
using SystemIntelligencePlatform.Incidents;

namespace SystemIntelligencePlatform.Dashboard;

public class DashboardDto
{
    public int TotalApplications { get; set; }
    public int TotalOpenIncidents { get; set; }
    public int TotalCriticalIncidents { get; set; }
    public long TotalLogsToday { get; set; }
    public Dictionary<string, int> SeverityDistribution { get; set; } = new();
    public List<IncidentTrendItemDto> IncidentTrend { get; set; } = new();
    public List<IncidentDto> RecentIncidents { get; set; } = new();
}

public class IncidentTrendItemDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
