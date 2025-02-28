using GenesisFEPortalWeb.Models.Entities.Common;
using GenesisFEPortalWeb.Models.Entities.Security;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenesisFEPortalWeb.Models.Entities.Tenant
{
    [Table("Tenants", Schema = "Core")]
    public class TenantModel : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Identification { get; set; } = null!;
        public string? CommercialName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public byte[]? Logo { get; set; }


        // Relaciones de navegación
        public virtual ICollection<UserModel> Users { get; set; } = new List<UserModel>();
        

    }
}
