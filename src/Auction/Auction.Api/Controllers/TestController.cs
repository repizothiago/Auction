using Microsoft.AspNetCore.Mvc;

namespace Auction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : Controller
{
    public TestController()
    {

    }

    [HttpGet]
    public async Task<IActionResult> Ping()
    {
        return Ok("Pong");
    }
}
