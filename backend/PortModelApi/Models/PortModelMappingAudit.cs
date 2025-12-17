using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortModelApi.Models;

[Table("port_model_mapping_audit", Schema = "crd")]
public class PortModelMappingAudit
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("accno_sleeve")]
    [MaxLength(50)]
    public string AccnoSleeve { get; set; } = default!;

    [Column("effectivedate")]
    public DateOnly EffectiveDate { get; set; }

    [Column("model_name")]
    [MaxLength(50)]
    public string? ModelName { get; set; }

    [Column("currency_model")]
    [MaxLength(1)]
    public string? CurrencyModel { get; set; }

    [Column("hedge_model_name")]
    [MaxLength(50)]
    public string? HedgeModelName { get; set; }

    [Column("action")]
    [MaxLength(1)]
    public string Action { get; set; } = default!;

    [Column("changed_by")]
    [MaxLength(100)]
    public string ChangedBy { get; set; } = default!;

    [Column("changed_at")]
    public DateTime ChangedAt { get; set; }
}
