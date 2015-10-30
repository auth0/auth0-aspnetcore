using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace WebApp
{
    public static class AuthenticationExtensions
    {
        private const string NonceProperty = "N";

        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();

        public static void ConfigureAuth0(this IServiceCollection services, string domain, string clientId, string clientSecret, string callbackUri, Func<AuthenticationValidatedContext, Task> onAuthenticationValidated = null)
        {
            if (onAuthenticationValidated == null)
            {
                onAuthenticationValidated = notification =>
                {
                    return Task.FromResult(true);
                };
            }

            services.Configure<OpenIdConnectOptions>(options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.Authority = $"https://{domain}";
                options.ResponseType = OpenIdConnectResponseTypes.Code;
                options.PostLogoutRedirectUri = callbackUri;

                options.Events = new OpenIdConnectEvents()
                {
                    OnRedirectToEndSessionEndpoint = notification =>
                    {
                        notification.HandleResponse();
                        notification.HttpContext.Response.Redirect($"https://{domain}/v2/logout?returnTo={notification.ProtocolMessage.RedirectUri}");
                        return Task.FromResult(true);
                    },
                    OnAuthenticationValidated = onAuthenticationValidated
                };
            });
        }

        public static void UseAuth0(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<OpenIdConnectOptions>>();
            app.UseOpenIdConnectAuthentication(options.Value);
        }

        public static AuthenticationTransaction PrepareAuthentication(this HttpContext context, string callbackUrl, string returnUrl)
        {
            var middlewareOptions = context.ApplicationServices.GetRequiredService<IOptions<OpenIdConnectOptions>>().Value;
            var nonce = middlewareOptions.ProtocolValidator.GenerateNonce();

            // Add the nonce.
            context.Response.Cookies.Append(
                OpenIdConnectDefaults.CookieNoncePrefix + middlewareOptions.StringDataFormat.Protect(nonce),
                NonceProperty,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = context.Request.IsHttps,
                    Expires = DateTime.UtcNow + middlewareOptions.ProtocolValidator.NonceLifetime
                });

            // Prepare state.
            var authenticationProperties = new AuthenticationProperties() { RedirectUri = returnUrl };
            AddCallbackUrl(callbackUrl, authenticationProperties);
            GenerateCorrelationId(context, middlewareOptions, authenticationProperties);

            // Generate state.
            var state =
                Uri.EscapeDataString(
                    middlewareOptions.StateDataFormat.Protect(authenticationProperties));

            // Return nonce to the Lock.
            return new AuthenticationTransaction(nonce, state);
        }

        private static void AddCallbackUrl(string callbackUrl, AuthenticationProperties properties)
        {
            properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey] = callbackUrl;
        }

        private static void GenerateCorrelationId(HttpContext context, OpenIdConnectOptions options, AuthenticationProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var correlationKey = OpenIdConnectDefaults.CookieStatePrefix;

            var nonceBytes = new byte[32];
            CryptoRandom.GetBytes(nonceBytes);
            var correlationId = Base64UrlTextEncoder.Encode(nonceBytes);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                Expires = DateTime.UtcNow + options.ProtocolValidator.NonceLifetime
            };

            properties.Items[correlationKey] = correlationId;

            context.Response.Cookies.Append(correlationKey + correlationId, NonceProperty, cookieOptions);
        }
    }
}
