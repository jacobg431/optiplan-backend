using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Optiplan.DatabaseResources;

[Table("Category")]
public partial class Category
{
    [Key]
    [Column(TypeName = "INT")]
    public int Id { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    public string Name { get; set; } = null!;

    [InverseProperty("Category")]
    public virtual ICollection<Dependency> Dependencies { get; set; } = new List<Dependency>();
}
