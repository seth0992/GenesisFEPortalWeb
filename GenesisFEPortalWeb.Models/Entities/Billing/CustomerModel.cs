using GenesisFEPortalWeb.Models.Entities.Catalog;
using GenesisFEPortalWeb.Models.Entities.Common;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Entities.Billing
{
    [Table("Customers", Schema = "Billing")]
    public class CustomerModel : BaseEntity, IHasTenant
    {
        public long TenantId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? CommercialName { get; set; }
        public string Identification { get; set; } = null!;
        public string IdentificationTypeId { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneCode { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Neighborhood { get; set; }
        public int? DistrictId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual TenantModel Tenant { get; set; } = null!;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual IdentificationTypeModel IdentificationType { get; set; } = null!;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual DistrictModel? District { get; set; }
    }
}
