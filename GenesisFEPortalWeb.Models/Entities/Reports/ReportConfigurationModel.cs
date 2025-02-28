using GenesisFEPortalWeb.Models.Entities.Common;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Entities.Reports
{
    [Table("ReportConfigurations", Schema = "Reports")]
    public class ReportConfigurationModel : BaseEntity, IHasTenant
    {
        public long TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!; // SALES, INVOICES, CUSTOMERS
        public string? Configuration { get; set; } // JSON con configuración específica
        public string? Schedule { get; set; } // Programación de generación automática
        public string Format { get; set; } = "PDF"; // PDF, CSV, EXCEL

        // Relaciones de navegación
        public virtual TenantModel Tenant { get; set; } = null!;
        public virtual ICollection<GeneratedReportModel> GeneratedReports { get; set; } = new List<GeneratedReportModel>();
    }
}
