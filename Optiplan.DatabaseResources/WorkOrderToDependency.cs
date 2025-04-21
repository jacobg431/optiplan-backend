using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Optiplan.DatabaseResources;

[PrimaryKey("DependencyInstanceId", "DependencyId", "WorkOrderId")]
[Table("WorkOrderToDependency")]
[Index("DependencyId", Name = "fk_WorkOrderToDependency_Dependency_idx")]
[Index("WorkOrderId", Name = "fk_WorkOrderToDependency_WorkOrder1_idx")]
public partial class WorkOrderToDependency
{
    [Key]
    [Column(TypeName = "INT")]
    [JsonPropertyName("dependencyInstanceId")]
    public int DependencyInstanceId { get; set; }

    [Key]
    [Column(TypeName = "INT")]
    [Required]
    [JsonPropertyName("dependencyId")]
    public int DependencyId { get; set; }

    [Key]
    [Column(TypeName = "INT")]
    [Required]
    [JsonPropertyName("workOrderId")]
    public int WorkOrderId { get; set; }

    [Column(TypeName = "VARCHAR(500)")]
    [StringLength(500)]
    [JsonPropertyName("textAttributeValue")]
    public string? TextAttributeValue { get; set; }

    [Column(TypeName = "INT")]
    [JsonPropertyName("integerAttributeValue")]
    public int? IntegerAttributeValue { get; set; }

    [Column(TypeName = "DOUBLE")]
    [JsonPropertyName("numberAttributeValue")]
    public double? NumberAttributeValue { get; set; }

    [Column(TypeName = "TINYINT")]
    [JsonPropertyName("booleanAttributeValue")]
    public byte? BooleanAttributeValue { get; set; }

    [Column(TypeName = "DATETIME")]
    [JsonPropertyName("startDateTime")]
    public DateTime? StartDateTime { get; set; }

    [Column(TypeName = "DATETIME")]
    [JsonPropertyName("stopDateTime")]
    public DateTime? StopDateTime { get; set; }

    [ForeignKey("DependencyId")]
    [InverseProperty("WorkOrderToDependencies")]
    [JsonIgnore]
    public virtual Dependency Dependency { get; set; } = null!;

    [ForeignKey("WorkOrderId")]
    [InverseProperty("WorkOrderToDependencies")]
    [JsonIgnore]
    public virtual WorkOrder WorkOrder { get; set; } = null!;
}
