using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssetManagement.Models.Enums;

namespace AssetManagement.Models.Entities
{
    [Table("t_assignments")]
    public class Assignment
    {
        [Key]
        public int AssignmentID { get; set; }

        public int AssetID { get; set; }

        [ForeignKey("AssetID")]
        public Asset? Asset { get; set; }

        public int AssignedToUserID { get; set; }

        [ForeignKey("AssignedToUserID")]
        public User? AssignedToUser { get; set; }

        public DateTime AssignedDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public AssignmentStatus Status { get; set; }

        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
