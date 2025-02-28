using GenesisFEPortalWeb.Models.Entities.Common;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Entities.Notifications
{
    [Table("Templates", Schema = "Notifications")]
    public class TemplateModel : BaseEntity, IHasTenant
    {
        public long TenantId { get; set; }
        public string Type { get; set; } = null!; // EMAIL, SYSTEM, SMS
        public string Name { get; set; } = null!;
        public string? Subject { get; set; }
        public string Template { get; set; } = null!;

        // Relaciones de navegación
        public virtual TenantModel Tenant { get; set; } = null!;
        public virtual ICollection<NotificationModel> Notifications { get; set; } = new List<NotificationModel>();
    }
}
