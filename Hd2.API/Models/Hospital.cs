using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hd2.API.Models;

[Table("Szpitale")]
public class Hospital
{
    [Key] public int Id { get; set; }
    [Required][StringLength(100)] public string Name { get; set; }

    // Navigation property to many or zero procedures
    public ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();

    // Navigation property to many or zero doctors
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    // Navigation property to one address
    public Address Address { get; set; }
    public int AddressId { get; set; }
}