using GenesisFEPortalWeb.Models.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Entities.Notifications
{
    [Table("NotificationDelivery", Schema = "Notifications")]
    public class NotificationDeliveryModel : BaseEntity
    {
        public long NotificationId { get; set; }
        public string DeliveryType { get; set; } = null!; // EMAIL, SMS, SYSTEM
        public string Status { get; set; } = null!;
        public string? ErrorMessage { get; set; }
        public DateTime? SentAt { get; set; }

        // Relaciones de navegación
        public virtual NotificationModel Notification { get; set; } = null!;
    }
}
