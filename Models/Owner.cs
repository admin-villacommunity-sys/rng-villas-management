using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VillaCommunityManagement.Models
{
    [Table("V_Owners")]
    public class Owner
    {
        [Key]
        public int Villa_No { get; set; }

        public string? Owner_name { get; set; }

        public string? Tenant_name { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Status { get; set; }
    }
}