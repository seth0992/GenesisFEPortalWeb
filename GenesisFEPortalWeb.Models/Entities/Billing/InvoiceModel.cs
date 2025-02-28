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
    [Table("Invoices", Schema = "Billing")]
    public class InvoiceModel : BaseEntity, IHasTenant
    {
        public long TenantId { get; set; }
        public long CustomerId { get; set; }
        public long DocumentTypeId { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public string ConsecutiveNumber { get; set; } = null!;
        public string KeyDocument { get; set; } = null!;
        public string? ReferenceDocument { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Currency { get; set; } = "CRC";
        public decimal ExchangeRate { get; set; } = 1;
        public int PaymentTerm { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public string? Notes { get; set; }
        public string? XmlDocument { get; set; }
        public byte[]? PdfDocument { get; set; }
        public bool IsVoided { get; set; }
        public string? VoidReason { get; set; }

        // Relaciones de navegación
        public virtual TenantModel Tenant { get; set; } = null!;
        public virtual CustomerModel Customer { get; set; } = null!;
        public virtual DocumentTypeModel DocumentType { get; set; } = null!;
        public virtual PaymentMethodModel PaymentMethodNavigation { get; set; } = null!;
        public virtual ICollection<InvoiceLineModel> InvoiceLines { get; set; } = new List<InvoiceLineModel>();
        public virtual ICollection<InvoiceStatusModel> InvoiceStatuses { get; set; } = new List<InvoiceStatusModel>();
    }
}
