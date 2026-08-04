using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VillaCommunityManagement.Models
{
    [Table("Income")]
    public class Income
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IncomeId { get; set; }

        public int month { get; set; }

        public string? income_source { get; set; }

        public decimal Amount { get; set; }

        public string? Income_Details { get; set; }
    }
}