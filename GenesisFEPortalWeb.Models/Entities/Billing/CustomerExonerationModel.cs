using GenesisFEPortalWeb.Models.Entities.Catalog;
using GenesisFEPortalWeb.Models.Entities.Common;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Entities.Billing
{
    [Table("CustomerExonerations", Schema = "Billing")]
    public class CustomerExonerationModel : BaseEntity, IHasTenant
    {
        public long TenantId { get; set; }
        public long CustomerId { get; set; }
        public string DocumentNumber { get; set; } = null!;
        public string DocumentType { get; set; } = null!;
        public long TaxTypeId { get; set; }
        public decimal ExonerationPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Relaciones de navegación
        public virtual TenantModel Tenant { get; set; } = null!;
        public virtual CustomerModel Customer { get; set; } = null!;
        public virtual TaxTypeModel TaxType { get; set; } = null!;
        public virtual ICollection<InvoiceLineModel> InvoiceLines { get; set; } = new List<InvoiceLineModel>();
    }
}
