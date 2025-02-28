using GenesisFEPortalWeb.Models.Entities.Common;
using GenesisFEPortalWeb.Models.Entities.Security;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenesisFEPortalWeb.Models.Entities.Audit
{
    [Table("ActivityLogs", Schema = "Audit")]
    public class ActivityLogModel : BaseEntity, IHasTenant
    {
        public long TenantId { get; set; }
        public long? UserId { get; set; }
        public string Action { get; set; } = null!;
        public string EntityName { get; set; } = null!;
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        // Relaciones de navegación
        public virtual TenantModel Tenant { get; set; } = null!;
        public virtual UserModel? User { get; set; }
    }
}
