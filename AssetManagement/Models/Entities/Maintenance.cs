using AssetManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AssetManagement.Models.Entities
{
    [Table("t_maintenance")]
    public class Maintenance
    {
        [Key]
        public int MaintenanceID { get; set; }

        public int AssetID { get; set; }

        [ForeignKey("AssetID")]
        public Asset? Asset { get; set; }

        public string? Description { get; set; }

        public DateTime? ScheduleDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MaintenanceStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

       
    }
}
