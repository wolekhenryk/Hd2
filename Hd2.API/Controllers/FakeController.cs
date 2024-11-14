using Hd2.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hd2.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FakeController(Generator generator) : ControllerBase {
    [HttpPost("t1")]
    public async Task<IActionResult> T1([FromQuery] int days)
    {
        await generator.GenerateT1(days);
        return Ok();
    }

    [HttpPost("t2")]
    public async Task<IActionResult> T2([FromQuery] int days)
    {
        await generator.GenerateT2(days);
        return Ok();
    }
}