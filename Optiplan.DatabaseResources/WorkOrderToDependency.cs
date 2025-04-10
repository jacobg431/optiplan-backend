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
    public int DependencyId { get; set; }

    [Key]
    [Column(TypeName = "INT")]
    public int WorkOrderId { get; set; }

    [Column(TypeName = "VARCHAR(500)")]
    public string? InputField1Value { get; set; }

    [Column(TypeName = "VARCHAR(500)")]
    public string? InputField2Value { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    public string? DropdownOptionSelected { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    public string? RadioOptionSelected { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    public string? CheckboxOptionSelected { get; set; }

    [Column(TypeName = "INT")]
    public int? SliderMinValue { get; set; }

    [Column(TypeName = "INT")]
    public int? SliderMaxValue { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? Start { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? End { get; set; }

    [ForeignKey("DependencyId")]
    [InverseProperty("WorkOrderToDependencies")]
    public virtual Dependency Dependency { get; set; } = null!;

    [ForeignKey("WorkOrderId")]
    [InverseProperty("WorkOrderToDependencies")]
    public virtual WorkOrder WorkOrder { get; set; } = null!;
}
