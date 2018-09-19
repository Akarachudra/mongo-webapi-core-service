using Microsoft.AspNetCore.Mvc;

namespace Mongo.Service.Core.Controllers.System
{
    [Route("_status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet("ping")]
        public PingResult GetPingResult()
        {
            return new PingResult { Status = "Ok" };
        }
    }
}