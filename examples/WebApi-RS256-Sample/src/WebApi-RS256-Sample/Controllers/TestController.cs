using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace WebApiSample
{
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Route("api/ping")]
        public object Ping()
        {
            return new
            {
                message = "Pong. You accessed an unprotected endpoint."
            };
        }

        [Authorize("Bearer")]
        [HttpGet]
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
