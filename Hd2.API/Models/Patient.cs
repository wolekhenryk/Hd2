using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hd2.API.Models;

[Table("Pacjenci")]
public class Patient : Person
{
    [Required][StringLength(5)] public string BloodType { get; set; }
    [Required] [StringLength(1000)] public string HealthStatus { get; set; }

    // Navigation property to many procedures
    public ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();
}