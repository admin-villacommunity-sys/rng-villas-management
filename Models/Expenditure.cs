using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VillaCommunityManagement.Models
{
    [Table("Expenditure")]
    public class Expenditure
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExpenditureId { get; set; }

        public DateTime Payment_date { get; set; }

        public string? payment_details { get; set; }

        public string? paid_by { get; set; }

        public decimal Amount { get; set; }
    }
}