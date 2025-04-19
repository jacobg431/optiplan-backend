using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "VARCHAR(500)")]
    [StringLength(500)]
    public string? Tooltip { get; set; }

    [Column(TypeName = "INT")]
    [Required]
    public int CategoryId { get; set; }

    [Column(TypeName = "TINYINT")]
    [DefaultValue(0)]
    public byte MultipleInstances { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    [StringLength(100)]
    public string? TextAttributeLabel { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    [StringLength(100)]
    public string? IntegerAttributeLabel { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    [StringLength(100)]
    public string? NumberAttributeLabel { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    [StringLength(100)]
    public string? BooleanAttributeLabel { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Dependencies")]
    public virtual Category Category { get; set; } = null!;

    [InverseProperty("Dependency")]
    public virtual ICollection<WorkOrderToDependency> WorkOrderToDependencies { get; set; } = new List<WorkOrderToDependency>();
}
