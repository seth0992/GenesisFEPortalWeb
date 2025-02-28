using GenesisFEPortalWeb.Models.Entities.Catalog;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenesisFEPortalWeb.Models.Catalog
{
    [Table("Region", Schema = "Catalog")]
    public class RegionModel
    {
        [Key]
        public int RegionID { get; set; }
        public string RegionName { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<DistrictModel>? Districts { get; }
    }
}
