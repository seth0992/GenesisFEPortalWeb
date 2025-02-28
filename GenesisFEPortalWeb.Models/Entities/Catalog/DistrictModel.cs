using GenesisFEPortalWeb.Models.Catalog;
using GenesisFEPortalWeb.Models.Entities.Billing;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenesisFEPortalWeb.Models.Entities.Catalog
{
    [Table("Districts", Schema = "Catalog")]
    public class DistrictModel
    {
        [Key]
        public int DistrictID { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public int CantonId { get; set; }
        public int RegionID { get; set; }

        public CantonModel? Canton { get; set; }
        public RegionModel? Region { get; set; }

        [JsonIgnore]
        public ICollection<CustomerModel>? CustomerModels { get; }
    }
}
