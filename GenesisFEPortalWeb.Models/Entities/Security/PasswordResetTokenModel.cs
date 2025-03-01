using GenesisFEPortalWeb.Models.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Entities.Security
{
    [Table("PasswordResetTokens", Schema = "Security")]
    public class PasswordResetTokenModel : BaseEntity
    {
        public long UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }

        // Relaciones de navegación
        public virtual UserModel User { get; set; } = null!;
    }
}
