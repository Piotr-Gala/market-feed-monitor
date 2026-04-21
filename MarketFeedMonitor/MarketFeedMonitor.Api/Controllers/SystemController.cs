using Microsoft.AspNetCore.Mvc;

namespace MarketFeedMonitor.Api.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new
        {
            status = "ok",
            service = "MarketFeedMonitor.Api",
            utcNow = DateTimeOffset.UtcNow
        });
    }
}
