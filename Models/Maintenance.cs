using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VillaCommunityManagement.Models
{
    [Table("Maintenance")]
    public class Maintenance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaintenanceId { get; set; }

        public int Villa_No { get; set; }

        public int Month { get; set; }

        [Required]
        public DateTime Due { get; set; }

        public decimal? paid { get; set; }

        [Required]
        public bool Payment_details { get; set; }

        public DateTime? payment_date { get; set; }
    }
}