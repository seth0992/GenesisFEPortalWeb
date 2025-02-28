using GenesisFEPortalWeb.Models.Entities.Common;
using GenesisFEPortalWeb.Models.Entities.Security;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenesisFEPortalWeb.Models.Entities.Audit
{
    [Table("SecurityLogs", Schema = "Audit")]
    public class SecurityLogsModel : BaseEntity
    {
        public long? TenantId { get; set; }
        public long? UserId { get; set; }
        public string EventType { get; set; } = null!;
        public string? Description { get; set; }
        public string? IpAddress { get; set; }

        // Relaciones de navegación
        public virtual TenantModel? Tenant { get; set; }
        public virtual UserModel? User { get; set; }
    }
}
