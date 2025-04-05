using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Optiplan.DatabaseResources;

[PrimaryKey("DependencyInstanceId", "DependencyId", "ProjectId")]
[Table("ProjectToDependency")]
[Index("DependencyId", Name = "fk_ProjectToDependency_Dependency_idx")]
[Index("ProjectId", Name = "fk_ProjectToDependency_Project1_idx")]
public partial class ProjectToDependency
{
    [Key]
    [Column(TypeName = "INT")]
    public int DependencyInstanceId { get; set; }

    [Key]
    [Column(TypeName = "INT")]
    public int DependencyId { get; set; }

    [Key]
    [Column(TypeName = "INT")]
    public int ProjectId { get; set; }

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
    [InverseProperty("ProjectToDependencies")]
    public virtual Dependency Dependency { get; set; } = null!;

    [ForeignKey("ProjectId")]
    [InverseProperty("ProjectToDependencies")]
    public virtual Project Project { get; set; } = null!;
}
