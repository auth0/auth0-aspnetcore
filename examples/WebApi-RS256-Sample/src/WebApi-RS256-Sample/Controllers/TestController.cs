using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;

namespace WebApiSample
{
    public class TestController : Controller
    {
        [HttpGet]
        [Route("api/ping")]
        public string Ping()
        {
            return "Pong. You accessed an unprotected endpoint.";
        }

        [HttpGet]
        [Authorize(ActiveAuthenticationSchemes = "Bearer")]
        [Route("api/secured/ping")]
        public object SecuredPing()
        {
            return new
            {
                message = "Pong. You accessed a protected endpoint.",
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            };
        }
    }
}
