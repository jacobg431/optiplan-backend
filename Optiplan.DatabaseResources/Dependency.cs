using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Optiplan.DatabaseResources;

[Table("Dependency")]
[Index("CategoryId", Name = "fk_Dependency_Category1_idx")]
public partial class Dependency
{
    [Key]
    [Column(TypeName = "INT")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    [StringLength(100)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [Column(TypeName = "VARCHAR(500)")]
    [StringLength(500)]
    [JsonPropertyName("tooltip")]
    public string? Tooltip { get; set; }

    [Column(TypeName = "INT")]
    [Required]
    [JsonPropertyName("categoryId")]
    public int CategoryId { get; set; }

    [Column(TypeName = "TINYINT")]
    [DefaultValue(0)]
    [JsonPropertyName("multipleInstances")]
    public byte MultipleInstances { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    [StringLength(100)]
    [JsonPropertyName("textAttributeLabel")]
    public string? TextAttributeLabel { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    [StringLength(100)]
    [JsonPropertyName("integerAttributeLabel")]
    public string? IntegerAttributeLabel { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    [StringLength(100)]
    [JsonPropertyName("numberAttributeLabel")]
    public string? NumberAttributeLabel { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    [StringLength(100)]
    [JsonPropertyName("booleanAttributeLabel")]
    public string? BooleanAttributeLabel { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Dependencies")]
    [JsonPropertyName("category")]
    public virtual Category Category { get; set; } = null!;

    [InverseProperty("Dependency")]
    [JsonIgnore]
    public virtual ICollection<WorkOrderToDependency> WorkOrderToDependencies { get; set; } = new List<WorkOrderToDependency>();
}
