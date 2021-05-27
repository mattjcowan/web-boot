using System;
using Microsoft.AspNetCore.Mvc;

namespace TestWebApp.Controllers
{
    [Route("api/ping")]
    [ApiController]
    public class PingController: ControllerBase
    {
        [HttpGet]
        public ActionResult Ping()
        {
            return new JsonResult(new {
                Ping = "Pong"
            });
        }
    }
}