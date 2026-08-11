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

        // NEW: Total amount due for this month
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DueAmount { get; set; } = 0;

        // Amount already paid (can be partial)
        public decimal? paid { get; set; }

        public bool Payment_details { get; set; }

        public DateTime? payment_date { get; set; }
    }
}