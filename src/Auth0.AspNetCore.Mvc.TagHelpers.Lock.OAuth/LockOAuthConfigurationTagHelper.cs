using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Authentication;

namespace Auth0.AspNetCore.Mvc.TagHelpers.Lock.OAuth
{
    [HtmlTargetElement("lock-oauth-configuration", ParentTag = "lock")]
    public class LockOAuthConfigurationTagHelper : LockRemoteConfigurationTagHelper
    {
        private readonly IServiceProvider _serviceProvider;

        public LockOAuthConfigurationTagHelper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var options = _serviceProvider.GetServices<IOptions<OAuthOptions>>().FirstOrDefault(o => o.Value.AuthenticationScheme == AuthenticationScheme)?.Value;

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
                    ExpiresUtc = options.SystemClock.UtcNow.Add(options.RemoteAuthenticationTimeout),
                    RedirectUri = ReturnUrl ?? "/"
                };
                GenerateCorrelationId(properties, options.AuthenticationScheme);

                // Generate State
                lockContext.State = Uri.EscapeDataString(options.StateDataFormat.Protect(properties));
            }

            output.SuppressOutput();

            return Task.FromResult(0);
        }
    }
}