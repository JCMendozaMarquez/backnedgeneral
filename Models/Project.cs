using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGeneral.Api.Models;

[Table("projects")]
public class Project
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("code")]
    public string? Code { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("status")]
    public string? Status { get; set; }
}