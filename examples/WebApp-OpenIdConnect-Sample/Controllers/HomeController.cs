namespace WebApp.Controllers
{
    using Microsoft.AspNet.Authorization;
    using Microsoft.AspNet.Mvc;

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return this.View();
        }

        [Authorize]
        public IActionResult Profile()
        {
            return this.View(this.User.Claims);
        }
    }
}
