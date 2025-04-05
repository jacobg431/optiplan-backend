using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Optiplan.DatabaseResources;

[Table("Project")]
public partial class Project
{
    [Key]
    [Column(TypeName = "INT")]
    public int Id { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    public string Name { get; set; } = null!;

    [Column(TypeName = "DATETIME")]
    public DateTime? Start { get; set; }

    [Column(TypeName = "DATETIME")]
    public DateTime? End { get; set; }

    [InverseProperty("Project")]
    public virtual ICollection<ProjectToDependency> ProjectToDependencies { get; set; } = new List<ProjectToDependency>();
}
