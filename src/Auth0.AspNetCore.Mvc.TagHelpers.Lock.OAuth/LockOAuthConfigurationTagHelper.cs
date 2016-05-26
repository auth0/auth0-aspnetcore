﻿using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authentication;
using System.Security.Cryptography;

namespace Auth0.AspNetCore.Mvc.TagHelpers.Lock.OAuth
{
    [HtmlTargetElement("lock-oauth-configuration", ParentTag = "lock")]
    public class LockOAuthConfigurationTagHelper : TagHelper
    {
        private const string CorrelationPrefix = ".AspNetCore.Correlation.";
        private const string CorrelationProperty = ".xsrf";
        private const string CorrelationMarker = "N";

        private readonly IServiceProvider _serviceProvider;

        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();

        public string AuthenticationScheme { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        private HttpRequest Request => ViewContext.HttpContext.Request;

        private HttpResponse Response => ViewContext.HttpContext.Response;

        public LockOAuthConfigurationTagHelper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var alloptions = _serviceProvider.GetServices<IOptions<OAuthOptions>>();
            var options = alloptions.FirstOrDefault(o => o.Value.AuthenticationScheme == AuthenticationScheme)?.Value;

            if (options != null)
            {
                var lockContext = (LockContext)context.Items[typeof(LockContext)];

                // Set the options
                lockContext.ClientId = options.ClientId;

                // retrieve the domain from the authority
                Uri authorityUri;
                if (Uri.TryCreate(options.AuthorizationEndpoint, UriKind.Absolute, out authorityUri))
                {
                    lockContext.Domain = authorityUri.Host;
                }

                // Set the redirect
                lockContext.CallbackUrl = BuildRedirectUri(options.CallbackPath);

                // Since we are handling the 1st leg of the Auth (redirecting to /authorize), we need to generate the correlation ID so the 
                // OAuth middleware can validate it correctly once it picks up from the 2nd leg (receiving the code)
                AuthenticationProperties properties = new AuthenticationProperties
                {
                    ExpiresUtc = options.SystemClock.UtcNow.Add(options.RemoteAuthenticationTimeout)
                };
                GenerateCorrelationId(properties, options);

                // Generate State
                lockContext.State = options.StateDataFormat.Protect(properties);
            }

            output.SuppressOutput();

            return Task.FromResult(0);
        }

        protected string BuildRedirectUri(PathString redirectPath)
        {
            return Request.Scheme + "://" + Request.Host + redirectPath;
        }

        protected virtual void GenerateCorrelationId(AuthenticationProperties properties, OAuthOptions options)
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