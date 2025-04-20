using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    public int DependencyInstanceId { get; set; }

    [Key]
    [Column(TypeName = "INT")]
    [Required]
    public int DependencyId { get; set; }

    [Key]
    [Column(TypeName = "INT")]
    [Required]
    public int WorkOrderId { get; set; }

    [Column(TypeName = "VARCHAR(500)")]
    [StringLength(500)]
    public string? TextAttributeValue { get; set; }

    [Column(TypeName = "INT")]
    public int? IntegerAttributeValue { get; set; }

    [Column(TypeName = "DOUBLE")]
    public double? NumberAttributeValue { get; set; }

    [Column(TypeName = "TINYINT")]
    public byte? BooleanAttributeValue { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? StartDateTime { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? StopDateTime { get; set; }

    [ForeignKey("DependencyId")]
    [InverseProperty("WorkOrderToDependencies")]
    public virtual Dependency Dependency { get; set; } = null!;

    [ForeignKey("WorkOrderId")]
    [InverseProperty("WorkOrderToDependencies")]
    public virtual WorkOrder WorkOrder { get; set; } = null!;
}
