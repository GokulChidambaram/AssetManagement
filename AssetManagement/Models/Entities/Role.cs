using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Models.Entities
{
    [Table("t_roles")]
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public bool IsDeleted { get; set; }= false;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

		public string CreatedBy { get; set; }

		public string UpdatedBy { get; set; }
	}
}
