using GenesisFEPortalWeb.Models.Catalog;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenesisFEPortalWeb.Models.Entities.Catalog
{
    [Table("Cantons", Schema = "Catalog")]
    public class CantonModel
    {
        [Key]
        public int CantonID { get; set; }
        public string CantonName { get; set; } = string.Empty;
        public int ProvinceId { get; set; }

        public ProvinceModel? Province { get; set; }

        [JsonIgnore]
        public ICollection<DistrictModel>? Districts { get; }

    }
}
