using AssetManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AssetManagement.Models.Entities
{
    [Table("t_users")]
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        public int RoleID { get; set; }

        [ForeignKey("RoleID")]
        public Role? Role { get; set; }

        public string? Department { get; set; }

        public string? PasswordHash { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

		public string CreatedBy { get; set; }

		public string UpdatedBy { get; set; }
	}
}
