
using GenesisFEPortalWeb.Models.Entities.Billing;
using GenesisFEPortalWeb.Models.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenesisFEPortalWeb.Models.Entities.Catalog
{
    [Table("TaxTypes", Schema = "Catalog")]
    public class TaxTypeModel : BaseEntity
    {
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Column(TypeName = "decimal(5,2)")]
        public decimal Rate { get; set; }

        public bool IsExemption { get; set; }

        // Relaciones de navegación
        public virtual ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();
        public virtual ICollection<CustomerExonerationModel> CustomerExonerations { get; set; } = new List<CustomerExonerationModel>();
        public virtual ICollection<InvoiceLineModel> InvoiceLines { get; set; } = new List<InvoiceLineModel>();
    }
}
