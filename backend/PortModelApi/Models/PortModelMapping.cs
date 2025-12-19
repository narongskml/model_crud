using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortModelApi.Models;

[Table("port_model_mapping", Schema = "crd")]
public class PortModelMapping
{
    [Key]
    [Column("accno_sleeve")]
    [MaxLength(20)]
    public string AccnoSleeve { get; set; } = default!;

    [Key]
    [Column("effectivedate")]
    public DateOnly EffectiveDate { get; set; }

    [Column("model_name")]
    [MaxLength(100)]
    public string ModelName { get; set; } = default!;

    [Column("currency_model")]
    [MaxLength(1)]
    public string? CurrencyModel { get; set; }

    [Column("hedge_model_name")]
    [MaxLength(100)]
    public string? HedgeModelName { get; set; }

    // Audit & soft delete
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("created_by")]
    [MaxLength(50)]
    public string? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_by")]
    [MaxLength(50)]
    public string? UpdatedBy { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_by")]
    [MaxLength(50)]
    public string? DeletedBy { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}