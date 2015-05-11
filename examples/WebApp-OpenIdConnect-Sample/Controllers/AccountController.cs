namespace WebApp.Controllers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.AspNet.Authentication.Cookies;
    using Microsoft.AspNet.Authentication.OpenIdConnect;
    using Microsoft.AspNet.Http;
    using Microsoft.AspNet.Http.Authentication;
    using Microsoft.AspNet.Mvc;
    using Microsoft.Framework.DependencyInjection;
    using Microsoft.Framework.OptionsModel;

    using WebApp.Model;
    using WebApp.Properties;

    public class AccountController : Controller
    {
        private readonly IOptions<Auth0Settings> auth0Settings;

        public AccountController(IOptions<Auth0Settings> auth0Settings)
        {
            this.auth0Settings = auth0Settings;
        }

        [Route("login")]
        public IActionResult Login(string returnUrl)
        {
            returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
            
            // Generate the nonce.
            var middlewareOptions = this.Context.ApplicationServices.GetService<IOptions<OpenIdConnectAuthenticationOptions>>();
            var nonce = middlewareOptions.Options.ProtocolValidator.GenerateNonce();

            // Store it in the cache or in a cookie.
            if (middlewareOptions.Options.NonceCache != null)
            {
                middlewareOptions.Options.NonceCache.AddNonce(nonce);
            }
            else
            {
                Response.Cookies.Append(
                    ".AspNet.OpenIdConnect.Nonce." + middlewareOptions.Options.StringDataFormat.Protect(nonce),
                        "N", new CookieOptions { HttpOnly = true, Secure = Request.IsHttps });
            }

            // Generate the state.
            var state =
                Uri.EscapeDataString(
                    middlewareOptions.Options.StateDataFormat.Protect(
                        new AuthenticationProperties(new Dictionary<string, string> { { ".redirect", returnUrl } })));

            // Return nonce to the Lock.
            return this.View(new LoginModel { ReturnUrl = returnUrl, Nonce = nonce, State = $"OpenIdConnect.AuthenticationProperties={state}" });
        }

        [HttpPost]
        [Route("logout")]
        public ActionResult Logout(string returnUrl)
        {
            if (this.Context.User.Identity.IsAuthenticated)
            {
                this.Context.Response.SignOut(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            var baseUrl = new Microsoft.AspNet.Http.Extensions.UriHelper(Request).GetFullUri();
            var absoluteReturnUrl = string.IsNullOrEmpty(returnUrl) ?
                this.Url.Action("Index", "Home", new { }, this.Request.Scheme) :
                this.Url.IsLocalUrl(returnUrl) ?
                    new Uri(new Uri(baseUrl), returnUrl).AbsoluteUri : returnUrl;
            return this.Redirect($"https://{this.auth0Settings.Options.Domain}/v2/logout?returnTo={absoluteReturnUrl}");
        }
    }
}
