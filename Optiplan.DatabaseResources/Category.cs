using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Optiplan.DatabaseResources;

[Table("Category")]
public partial class Category
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

    [InverseProperty("Category")]
    [JsonIgnore]
    public virtual ICollection<Dependency> Dependencies { get; set; } = new List<Dependency>();
}
