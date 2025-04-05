using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Optiplan.DatabaseResources;

[Table("Currency")]
public partial class Currency
{
    [Key]
    [Column(TypeName = "INT")]
    public int Id { get; set; }

    [Column(TypeName = "VARCHAR(100)")]
    public string Country { get; set; } = null!;

    [Column(TypeName = "CHAR(3)")]
    public string Code { get; set; } = null!;
}
