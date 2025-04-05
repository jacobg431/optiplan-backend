using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Optiplan.DatabaseResources;

[Table("Dependency")]
[Index("CategoryId", Name = "fk_Dependency_Category1_idx")]
public partial class Dependency
{
    [Key]
    [Column(TypeName = "INT")]
    public int Id { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    public string Name { get; set; } = null!;

    [Column(TypeName = "VARCHAR(500)")]
    public string? Tooltip { get; set; }

    [Column(TypeName = "INT")]
    public int CategoryId { get; set; }

    [Column(TypeName = "TINYINT")]
    public byte MultipleInstances { get; set; }

    [Column(TypeName = "TINYINT")]
    public byte InputField1 { get; set; }

    [Column(TypeName = "VARCHAR(500)")]
    public string? InputFieldLabel { get; set; }

    [Column(TypeName = "TINYINT")]
    public byte InputField2 { get; set; }

    [Column(TypeName = "VARCHAR(500)")]
    public string? InputField2Label { get; set; }

    [Column(TypeName = "TINYINT")]
    public byte DropdownMenu { get; set; }

    [Column(TypeName = "VARCHAR(500)")]
    public string? DropdownOptions { get; set; }

    [Column(TypeName = "TINYINT")]
    public byte RadioButtons { get; set; }

    [Column(TypeName = "VARCHAR(500)")]
    public string? RadioOptions { get; set; }

    [Column(TypeName = "TINYINT")]
    public byte CheckboxButtons { get; set; }

    [Column(TypeName = "VARCHAR(500)")]
    public string? CheckboxOptions { get; set; }

    [Column(TypeName = "TINYINT")]
    public byte Slider { get; set; }

    [Column(TypeName = "INT")]
    public int? SliderMin { get; set; }

    [Column(TypeName = "INT")]
    public int? SliderMax { get; set; }

    [Column(TypeName = "TINYINT")]
    public byte SliderRange { get; set; }

    [Column(TypeName = "TINYINT")]
    public byte DateTimePicker { get; set; }

    [Column(TypeName = "TINYINT")]
    public byte DateTimeRange { get; set; }

    [Column(TypeName = "INT")]
    public int? RequiredDependencyId { get; set; }

    [Column(TypeName = "INT")]
    public int? RecommendedDependencyId { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Dependencies")]
    public virtual Category Category { get; set; } = null!;

    [InverseProperty("Dependency")]
    public virtual ICollection<ProjectToDependency> ProjectToDependencies { get; set; } = new List<ProjectToDependency>();
}
