using System;
using Microsoft.AspNetCore.Mvc;

namespace TestWebApp.Controllers
{
    [Route("api/info")]
    [ApiController]
    public class InfoController: ControllerBase
    {
        [HttpGet]
        public ActionResult GetInfo()
        {
            return new JsonResult(new {
                App = typeof(InfoController).Assembly.GetName().Name,
                Version = typeof(InfoController).Assembly.GetName().Version.ToString(3),
                Uptime = (DateTime.UtcNow - Program.StartTime).ToString()
            });
        }
    }
}