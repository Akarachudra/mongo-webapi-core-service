using System.Web.Http;

namespace Mongo.Service.Core.Controllers.System
{
    [RoutePrefix("_status")]
    public class StatusController : ApiController
    {
        [Route("ping")]
        public PingResult GetPingResult()
        {
            return new PingResult { Status = "Ok" };
        }
    }

    public class PingResult
    {
        public string Status { get; set; }
    }
}