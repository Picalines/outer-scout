using Microsoft.AspNetCore.Mvc;

namespace Picalines.OuterWilds.SceneRecorder.WebInterop.Controllers;

internal sealed class IndexController : Controller
{
    [HttpGet("api/index")]
    public IActionResult Index()
    {
        return Ok(new { Data = "Hello world!" });
    }
}
