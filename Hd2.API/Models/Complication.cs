using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hd2.API.Models;

[Table("Komplikacje")]
public class Complication
{
    [Key] public int Id { get; set; }

    [Required] [StringLength(1000)] public string Description { get; set; }

    [Required] [StringLength(255)] public string Type { get; set; }

    // Navigation property to procedure
    public Procedure Procedure { get; set; }
    public int ProcedureId { get; set; }
}