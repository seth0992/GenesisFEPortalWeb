using GenesisFEPortalWeb.Models.Entities.Billing;
using GenesisFEPortalWeb.Models.Entities.Common;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenesisFEPortalWeb.Models.Entities.Catalog
{
    [Table("Products", Schema = "Catalog")]
    public class ProductModel : BaseEntity, IHasTenant
    {
        public long TenantId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string CabysCode { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [StringLength(50)]
        public string UnitOfMeasure { get; set; } = null!;

        public long TaxTypeId { get; set; }

        public bool IsService { get; set; }

        // Relaciones de navegación
        public virtual TenantModel Tenant { get; set; } = null!;
        public virtual TaxTypeModel TaxType { get; set; } = null!;
        public virtual ICollection<InvoiceLineModel> InvoiceLines { get; set; } = new List<InvoiceLineModel>();
    }
}
