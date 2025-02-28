using GenesisFEPortalWeb.Models.Entities.Common;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Entities.Core
{
    [Table("DigitalCertificates", Schema = "Core")]
    public class DigitalCertificateModel : BaseEntity, IHasTenant
    {
        public long TenantId { get; set; }
        public byte[] CertificateData { get; set; } = null!;
        public string? Password { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int NotificationsSent { get; set; }
        public DateTime? LastNotificationDate { get; set; }

        // Relaciones de navegación
        public virtual TenantModel Tenant { get; set; } = null!;
    }
}
