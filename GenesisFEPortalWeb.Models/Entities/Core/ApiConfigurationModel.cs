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
    [Table("ApiConfiguration", Schema = "Core")]
    public class ApiConfigurationModel : BaseEntity, IHasTenant
    {
        public long TenantId { get; set; }
        public string ApiKey { get; set; } = null!;
        public string ApiSecret { get; set; } = null!;
        public string Environment { get; set; } = null!;
        public string? EndpointUrl { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;

        // Relaciones de navegación
        public virtual TenantModel Tenant { get; set; } = null!;
    }
}
