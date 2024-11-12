using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hd2.API.Models;

[Table("TypyOrganow")]
public class OrganType 
{
    [Key] public int Id { get; set; }
    [Required][StringLength(100)] public string Name { get; set; }

    // Navigation property to organs
    public ICollection<Organ> Organs { get; set; } = new List<Organ>();
}