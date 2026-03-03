using AssetManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AssetManagement.Models.Entities
{
    [Table("t_assets")]
    public class Asset
    {
        [Key]
        public int AssetID { get; set; }

        public string ModelNo { get; set; }

        public string Description { get; set; }

        public string DepartmentName {  get; set; }

        public string SupplierName { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public int CategoryID { get; set; }

        [ForeignKey(nameof(CategoryID))]
        [JsonIgnore]
        public Category? Category { get; set; }

        public string? Tag { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public decimal? Cost { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AssetStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
