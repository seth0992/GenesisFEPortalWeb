using GenesisFEPortalWeb.Models.Entities.Common;
using GenesisFEPortalWeb.Models.Entities.Security;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Entities.Reports
{
    [Table("GeneratedReports", Schema = "Reports")]
    public class GeneratedReportModel : BaseEntity, IHasTenant
    {
        public long TenantId { get; set; }
        public long ConfigurationId { get; set; }
        public long GeneratedBy { get; set; }
        public string? FilePath { get; set; }
        public string Status { get; set; } = null!;
        public DateTime GeneratedAt { get; set; }

        // Relaciones de navegación
        public virtual TenantModel Tenant { get; set; } = null!;
        public virtual ReportConfigurationModel Configuration { get; set; } = null!;
        public virtual UserModel GeneratedByUser { get; set; } = null!;
    }
}
