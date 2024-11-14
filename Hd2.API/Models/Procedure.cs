using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hd2.API.Models;

[Table("Zabiegi")]
public class Procedure
{
    [Key] public int Id { get; set; }
    [Required] public DateTime StartDateTime { get; set; }
    [Required] public DateTime EndDateTime { get; set; }

    // Navigation property to patient
    public Patient Patient { get; set; }
    public string PatientPesel { get; set; }

    // Navigation property to many doctors
    public ICollection<DoctorProcedure> DoctorProcedures { get; set; } = new List<DoctorProcedure>();

    // Navigation property to many or zero complications
    public ICollection<Complication> Complications { get; set; } = new List<Complication>();

    // Navigation property to hospital
    public Hospital Hospital { get; set; }
    public int HospitalId { get; set; }

    // Navigation property to one organ
    public Organ Organ { get; set; }
    public int OrganId { get; set; }
}