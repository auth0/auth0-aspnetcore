using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
    public class SampleApiController : Controller
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

        [HttpGet]
        [Authorize(ActiveAuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
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
