using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Models.Entities
{
    [Table("t_reports")]
    public class Report
    {
        [Key]
        public int ReportID { get; set; }

        public string? Scope { get; set; }

        public string? Metrics { get; set; }

        public DateTime GeneratedDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

		public string CreatedBy { get; set; }

		public string UpdatedBy { get; set; }
	}
}
