using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Optiplan.DatabaseResources;

[Table("WorkOrder")]
public partial class WorkOrder
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

    [InverseProperty("WorkOrder")]
    public virtual ICollection<WorkOrderToDependency> WorkOrderToDependencies { get; set; } = new List<WorkOrderToDependency>();
}
