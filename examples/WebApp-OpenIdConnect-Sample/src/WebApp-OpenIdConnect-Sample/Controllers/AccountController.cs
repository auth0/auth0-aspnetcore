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
    using System.Threading.Tasks;
    using Microsoft.Framework.Caching.Distributed;

    public class AccountController : Controller
    {
        private readonly IOptions<Auth0Settings> auth0Settings;

        public AccountController(IOptions<Auth0Settings> auth0Settings)
        {
            this.auth0Settings = auth0Settings;
        }
        
        public async Task<IActionResult> Login(string returnUrl)
        {
            returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";

            // Generate the nonce.
            var middlewareOptions = this.Context.ApplicationServices.GetService<IOptions<OpenIdConnectAuthenticationOptions>>();
            var nonce = middlewareOptions.Options.ProtocolValidator.GenerateNonce();

            // Store it in the cache or in a cookie.
            if (middlewareOptions.Options.NonceCache != null && middlewareOptions.Options.CacheNonces)
            {
                await middlewareOptions.Options.NonceCache.SetAsync(nonce, new byte[0], new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = middlewareOptions.Options.ProtocolValidator.NonceLifetime
                });
            }
            else
            {
                Response.Cookies.Append(
                    OpenIdConnectAuthenticationDefaults.CookieNoncePrefix + middlewareOptions.Options.StringDataFormat.Protect(nonce),
                        "N", new CookieOptions { HttpOnly = true, Secure = Request.IsHttps });
            }

            // Generate the state.
            var state =
                Uri.EscapeDataString(
                    middlewareOptions.Options.StateDataFormat.Protect(
                        new AuthenticationProperties(new Dictionary<string, string> { { ".redirect", returnUrl } })));

            // Return nonce to the Lock.
            return this.View(new LoginModel { ReturnUrl = returnUrl, Nonce = nonce, State = state });
        }

        [HttpPost]
        public ActionResult Logout(string returnUrl)
        {
            if (this.Context.User.Identity.IsAuthenticated)
            {
                this.Context.Authentication.SignOutAsync(OpenIdConnectAuthenticationDefaults.AuthenticationScheme);
                this.Context.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            var baseUrl = Microsoft.AspNet.Http.Extensions.UriHelper.Encode(Context.Request.Scheme, Context.Request.Host, Context.Request.PathBase, Context.Request.Path, Context.Request.QueryString);
            var absoluteReturnUrl = string.IsNullOrEmpty(returnUrl) ?
                this.Url.Action("Index", "Home", new { }, this.Request.Scheme) :
                this.Url.IsLocalUrl(returnUrl) ?
                    new Uri(new Uri(baseUrl), returnUrl).AbsoluteUri : returnUrl;
            return this.Redirect($"https://{this.auth0Settings.Options.Domain}/v2/logout?returnTo={absoluteReturnUrl}");
        }
    }
}