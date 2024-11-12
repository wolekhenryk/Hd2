using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hd2.API.Models;

[Table("Dawcy")]
public class Donor : Person
{
    [Required][StringLength(5)] public string BloodType { get; set; }
    [Required][StringLength(1000)] public string HealthStatus { get; set; }
    [Required] public bool AliveDuringExtraction { get; set; }

    // Navigation property to many organs
    public ICollection<Organ> Organs { get; set; } = new List<Organ>();
}