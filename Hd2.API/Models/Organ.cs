using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hd2.API.Models;

[Table("Narzady")]
public class Organ
{
    [Key] public int Id { get; set; }

    [Required] public DateTime HarvestDateTime { get; set; }
    [Required] [StringLength(100)] public string StorageType { get; set; }

    // Navigation property to organ type
    public OrganType OrganType { get; set; }
    public int OrganTypeId { get; set; }

    // Navigation property to single donor
    public Donor Donor { get; set; }
    public string DonorPesel { get; set; }

    // Navigation property to one procedure
    public Procedure Procedure { get; set; }

    [NotMapped]
    public DateTime ScheduledProcedureDate { get; set; }
}