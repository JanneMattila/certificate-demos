using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class WhoAmIController : ControllerBase
{
    private readonly ILogger<WhoAmIController> _logger;

    public WhoAmIController(ILogger<WhoAmIController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public string? Get()
    {
        // Set breakpoint here and inspect context
        return User.Identity?.Name;
    }
}
