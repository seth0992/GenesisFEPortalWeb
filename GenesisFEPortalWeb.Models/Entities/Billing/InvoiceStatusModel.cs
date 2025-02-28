using GenesisFEPortalWeb.Models.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Entities.Billing
{
    [Table("InvoiceStatus", Schema = "Billing")]
    public class InvoiceStatusModel : BaseEntity
    {
        public long InvoiceId { get; set; }
        public string Status { get; set; } = null!;
        public string? StatusDetail { get; set; }
        public string? HaciendaResponse { get; set; }

        // Relaciones de navegación
        public virtual InvoiceModel Invoice { get; set; } = null!;
    }
}
