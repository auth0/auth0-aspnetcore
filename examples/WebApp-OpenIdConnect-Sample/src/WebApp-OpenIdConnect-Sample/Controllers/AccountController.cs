namespace WebApp.Controllers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.AspNet.Authentication.Cookies;
    using Microsoft.AspNet.Authentication.OpenIdConnect;
    using Microsoft.AspNet.Http;
    using Microsoft.AspNet.Http.Authentication;
    using Microsoft.AspNet.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.OptionsModel;

    using WebApp.Model;
    using WebApp.Properties;
    using Microsoft.AspNet.Authentication;
    using System.Security.Cryptography;

    public class AccountController : Controller
    {
        private const string NonceProperty = "N";

        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();

        private readonly IOptions<Auth0Settings> auth0Settings;

        public AccountController(IOptions<Auth0Settings> auth0Settings)
        {
            this.auth0Settings = auth0Settings;
        }
        
        public IActionResult Login(string returnUrl)
        {
            returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";

            var transaction = HttpContext.PrepareAuthentication(auth0Settings.Value.RedirectUri, returnUrl);

            // Return nonce to the Lock.
            return this.View(new LoginModel { ReturnUrl = returnUrl, Nonce = transaction.Nonce, State = transaction.State });
        }

        [HttpPost]
        public ActionResult Logout(string returnUrl)
        {
            var baseUrl = Microsoft.AspNet.Http.Extensions.UriHelper.Encode(HttpContext.Request.Scheme, HttpContext.Request.Host, HttpContext.Request.PathBase, HttpContext.Request.Path, HttpContext.Request.QueryString);
            var absoluteReturnUrl = string.IsNullOrEmpty(returnUrl) ?
                Url.Action("Index", "Home", new { }, Request.Scheme) :
                Url.IsLocalUrl(returnUrl) ?
                    new Uri(new Uri(baseUrl), returnUrl).AbsoluteUri : returnUrl;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                HttpContext.Authentication.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties() { RedirectUri = absoluteReturnUrl });
                HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return Redirect("/");
        }
    }
}