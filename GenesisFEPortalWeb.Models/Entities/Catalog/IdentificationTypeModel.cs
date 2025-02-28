using GenesisFEPortalWeb.Models.Entities.Billing;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenesisFEPortalWeb.Models.Entities.Catalog
{
    [Table("IdentificationTypes", Schema = "Catalog")]
    public class IdentificationTypeModel
    {
        public string ID { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<CustomerModel>? Customers { get; }
    }
}
