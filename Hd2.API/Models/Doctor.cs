using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hd2.API.Models;

[Table("Lekarze")]
public class Doctor : Person
{
    [StringLength(50)] [Required] public string Pwz { get; set; }
    [StringLength(50)] [Required] public string Specialization { get; set; }

    // Navigation property to many or zero procedures
    public ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();

    // Navigation property to one hospital
    public Hospital Hospital { get; set; }
    public int HospitalId { get; set; }
}