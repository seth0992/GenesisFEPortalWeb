using GenesisFEPortalWeb.Models.Entities.Catalog;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenesisFEPortalWeb.Models.Catalog
{
    [Table("Provinces", Schema = "Catalog")]
    public class ProvinceModel
    {
        [Key]
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<CantonModel>? Cantons { get; }

    }
}
