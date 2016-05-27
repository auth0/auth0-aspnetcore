using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Security.Cryptography;

namespace Auth0.AspNetCore.Mvc.TagHelpers
{
    public class LockRemoteConfigurationTagHelper : TagHelper
    {
        protected static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();
        protected const string CorrelationPrefix = ".AspNetCore.Correlation.";
        protected const string CorrelationProperty = ".xsrf";
        protected const string CorrelationMarker = "N";
        protected HttpRequest Request => ViewContext.HttpContext.Request;
        protected HttpResponse Response => ViewContext.HttpContext.Response;

        public string AuthenticationScheme { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        protected string BuildRedirectUri(PathString redirectPath)
        {
            return Request.Scheme + "://" + Request.Host + redirectPath;
        }

        protected virtual void GenerateCorrelationId(AuthenticationProperties properties, string authenticationScheme)
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

            var cookieName = CorrelationPrefix + authenticationScheme + "." + correlationId;

            Response.Cookies.Append(cookieName, CorrelationMarker, cookieOptions);
        }
    }
}