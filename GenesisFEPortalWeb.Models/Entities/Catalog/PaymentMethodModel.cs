using GenesisFEPortalWeb.Models.Entities.Billing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenesisFEPortalWeb.Models.Entities.Catalog
{
    [Table("PaymentMethods", Schema = "Catalog")]
    public class PaymentMethodModel
    {
        [Key]
        [StringLength(2)]
        public string ID { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Description { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        // Relaciones de navegación
        public virtual ICollection<InvoiceModel> Invoices { get; set; } = new List<InvoiceModel>();
    }
}
