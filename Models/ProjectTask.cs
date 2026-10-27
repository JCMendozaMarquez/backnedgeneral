using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGeneral.Api.Models;

[Table("project_tasks")]
public class ProjectTask
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [Column("code")]
    public string? Code { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("weight", TypeName = "numeric(5,2)")]
    public decimal? Weight { get; set; }

    [Column("planned_start")]
    public DateOnly? PlannedStart { get; set; }

    [Column("planned_end")]
    public DateOnly? PlannedEnd { get; set; }

    [Column("actual_start")]
    public DateOnly? ActualStart { get; set; }

    [Column("actual_end")]
    public DateOnly? ActualEnd { get; set; }

    [Column("progress", TypeName = "numeric(5,2)")]
    public decimal? Progress { get; set; }

    [Column("status")]
    public string? Status { get; set; }
}