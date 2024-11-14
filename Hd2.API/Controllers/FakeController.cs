using Hd2.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hd2.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FakeController(Generator generator) : ControllerBase {
    [HttpPost("t1")]
    public async Task<IActionResult> T1([FromQuery] int days)
    {
        var csv = await generator.GenerateT1(days);
        return File(csv.ToArray(), "text/csv", "t1.csv");
    }

    [HttpPost("t2")]
    public async Task<IActionResult> T2([FromQuery] int days, IFormFile csv)
    {
        var ms = new MemoryStream();
        await csv.CopyToAsync(ms);

        ms.Position = 0;

        var newCsv = await generator.GenerateT2(days, ms);
        return File(newCsv.ToArray(), "text/csv", "t2.csv");
    }
}