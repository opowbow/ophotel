using Microsoft.AspNetCore.Mvc;

namespace hotels.Controllers
{
    [Route("")]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hotels Api - use /hotel to get a random hotel");
        }
    }
}
