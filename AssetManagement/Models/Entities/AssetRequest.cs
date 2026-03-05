using AssetManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Models.Entities
{
    public class AssetRequest
    {
        [Key]
        public int RequestID { get; set; }

        // The Employee picks a Category (Laptop), not a specific serial number
        public int CategoryID { get; set; }
        public int RequestedByUserID { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        // Default to 'Pending' (0: Pending, 1: Approved, 2: Rejected)
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        // Audit Fields (Following your existing pattern)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;

        // Navigation Properties
        public Category? Category { get; set; }
        public User? RequestedBy { get; set; }
    }
}
