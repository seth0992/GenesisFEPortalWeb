using GenesisFEPortalWeb.Models.Entities.Catalog;
using GenesisFEPortalWeb.Models.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Entities.Billing
{
    [Table("InvoiceLines", Schema = "Billing")]
    public class InvoiceLineModel : BaseEntity
    {
        public long InvoiceId { get; set; }
        public long ProductId { get; set; }
        public int LineNumber { get; set; }
        public string Description { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal SubTotal { get; set; }
        public long TaxTypeId { get; set; }
        public decimal TaxAmount { get; set; }
        public long? ExonerationId { get; set; }
        public decimal Total { get; set; }

        // Relaciones de navegación
        public virtual InvoiceModel Invoice { get; set; } = null!;
        public virtual ProductModel Product { get; set; } = null!;
        public virtual TaxTypeModel TaxType { get; set; } = null!;
        public virtual CustomerExonerationModel? Exoneration { get; set; }
    }
}
