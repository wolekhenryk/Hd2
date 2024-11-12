using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hd2.API.Models;

[Table("Adresy")]
public class Address
{
    [Key] public int Id { get; set; }
    [Required] [StringLength(100)] public string Region { get; set; }
    [Required] [StringLength(100)] public string City { get; set; }
    [Required] [StringLength(100)] public string Street { get; set; }
    [Required] [Range(1, int.MaxValue)] public int HouseNumber { get; set; }

    // Navigation property to many or zero people
    public ICollection<Person> People { get; set; } = new List<Person>();

    // Navigation property to one hospital
    public Hospital Hospital { get; set; }
}