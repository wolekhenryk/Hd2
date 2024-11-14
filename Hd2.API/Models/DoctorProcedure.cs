using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hd2.API.Models;

[Table("LekarzZabieg")]
public class DoctorProcedure {
    [Key] public int Id { get; set; }
    public string DoctorPesel { get; set; }
    public Doctor Doctor { get; set; }

    public int ProcedureId { get; set; }
    public Procedure Procedure { get; set; }
}