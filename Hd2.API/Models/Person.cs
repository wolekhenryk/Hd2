using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hd2.API.Models;

[Table("Osoby")]
public class Person
{
    [Key] [StringLength(11)] public string Pesel { get; set; }

    [StringLength(50)] public string FirstName { get; set; }

    [StringLength(50)] public string LastName { get; set; }

    public DateOnly BirthDate { get; set; }

    // Navigation property to one address
    public Address Address { get; set; }
    public int AddressId { get; set; }
}