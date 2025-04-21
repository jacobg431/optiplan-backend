using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Optiplan.DatabaseResources;

[Table("WorkOrder")]
public partial class WorkOrder
{
    [Key]
    [Column(TypeName = "INT")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    [Required]
    [StringLength(100)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [Column(TypeName = "DATETIME")]
    [JsonPropertyName("startDateTime")]
    public DateTime? StartDateTime { get; set; }

    [Column(TypeName = "DATETIME")]
    [JsonPropertyName("stopDateTime")]
    public DateTime? StopDateTime { get; set; }

    [InverseProperty("WorkOrder")]
    [JsonIgnore]
    public virtual ICollection<WorkOrderToDependency> WorkOrderToDependencies { get; set; } = new List<WorkOrderToDependency>();
}
