using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace Auth0.AspNetCore.Mvc.TagHelpers.Lock.OpenIdConnect
{
    [HtmlTargetElement("lock-openidconnect-configuration", ParentTag = "lock")]
    public class LockOpenIdConnectConfigurationTagHelper : LockRemoteConfigurationTagHelper
    {
        private const string NonceProperty = "N";

        private readonly IServiceProvider _serviceProvider;

        public LockOpenIdConnectConfigurationTagHelper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var options = _serviceProvider.GetServices<IOptions<OpenIdConnectOptions>>().FirstOrDefault(o => o.Value.AuthenticationScheme == AuthenticationScheme)?.Value;

            if (options != null)
            {
                var lockContext = (LockContext)context.Items[typeof(LockContext)];

                // Set the options
                lockContext.ClientId = options.ClientId;

                // retrieve the domain from the authority
                Uri authorityUri;
                if (Uri.TryCreate(options.Authority, UriKind.Absolute, out authorityUri))
                {
                    lockContext.Domain = authorityUri.Host;
                }

                // Set the redirect
                string callbackUrl = BuildRedirectUri(options.CallbackPath);
                lockContext.CallbackUrl = callbackUrl;

                // Add the nonce.
                var nonce = options.ProtocolValidator.GenerateNonce();
                Response.Cookies.Append(
                    OpenIdConnectDefaults.CookieNoncePrefix + options.StringDataFormat.Protect(nonce),
                    NonceProperty,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsHttps,
                        Expires = DateTime.UtcNow + options.ProtocolValidator.NonceLifetime
                    });
                lockContext.Nonce = nonce;

                // Since we are handling the 1st leg of the Auth (redirecting to /authorize), we need to generate the correlation ID so the 
                // OAuth middleware can validate it correctly once it picks up from the 2nd leg (receiving the code)
                var properties = new AuthenticationProperties()
                {
                    ExpiresUtc = options.SystemClock.UtcNow.Add(options.RemoteAuthenticationTimeout),
                    RedirectUri = ReturnUrl ?? "/"
                };
                properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey] = callbackUrl;
                GenerateCorrelationId(properties, options.AuthenticationScheme);

                // Generate State
                lockContext.State = Uri.EscapeDataString(options.StateDataFormat.Protect(properties));
            }

            output.SuppressOutput();
            return Task.FromResult(0);
        }
    }
}