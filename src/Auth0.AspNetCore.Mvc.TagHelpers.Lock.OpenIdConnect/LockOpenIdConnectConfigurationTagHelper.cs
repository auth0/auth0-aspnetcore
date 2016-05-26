using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace Auth0.AspNetCore.Mvc.TagHelpers.Lock.OpenIdConnect
{
    [HtmlTargetElement("lock-openidconnect-configuration", ParentTag = "lock")]
    public class LockOpenIdConnectConfigurationTagHelper : TagHelper
    {
        private const string CorrelationPrefix = ".AspNetCore.Correlation.";
        private const string CorrelationProperty = ".xsrf";
        private const string CorrelationMarker = "N";
        private const string NonceProperty = "N";

        private readonly IServiceProvider _serviceProvider;
        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();
        private HttpRequest Request => ViewContext.HttpContext.Request;
        private HttpResponse Response => ViewContext.HttpContext.Response;

        public string AuthenticationScheme { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

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
                    RedirectUri = "/"
                };
                properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey] = callbackUrl;
                GenerateCorrelationId(properties, options);

                // Generate State
                lockContext.State = Uri.EscapeDataString(options.StateDataFormat.Protect(properties));
            }

            output.SuppressOutput();
            return Task.FromResult(0);
        }

        protected string BuildRedirectUri(PathString redirectPath)
        {
            return Request.Scheme + "://" + Request.Host + redirectPath;
        }

        protected virtual void GenerateCorrelationId(AuthenticationProperties properties, OpenIdConnectOptions options)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var bytes = new byte[32];
            CryptoRandom.GetBytes(bytes);
            var correlationId = Base64UrlTextEncoder.Encode(bytes);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                Expires = properties.ExpiresUtc
            };

            properties.Items[CorrelationProperty] = correlationId;

            var cookieName = CorrelationPrefix + options.AuthenticationScheme + "." + correlationId;

            Response.Cookies.Append(cookieName, CorrelationMarker, cookieOptions);
        }
    }
}