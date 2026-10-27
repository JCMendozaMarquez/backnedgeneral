using Microsoft.EntityFrameworkCore;

namespace BackendGeneral.Api.Models;

[Keyless]
public class ProjectSCurvePoint
{
    public Guid ProjectId { get; set; }
    public DateOnly MonthStart { get; set; }
    public string MonthKey { get; set; } = string.Empty;
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Planned { get; set; }
    public decimal Actual { get; set; }
}