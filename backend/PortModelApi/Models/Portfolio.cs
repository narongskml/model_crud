using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortModelApi.Models;

[Table("Portfolio", Schema = "dro")]
public class Portfolio
{
    [Key]
    [Column("Code")]
    [MaxLength(50)]
    public string Code { get; set; } = default!;

    [Column("Name")] 
    [MaxLength(100)]
    public string? Name { get; set; }
}
