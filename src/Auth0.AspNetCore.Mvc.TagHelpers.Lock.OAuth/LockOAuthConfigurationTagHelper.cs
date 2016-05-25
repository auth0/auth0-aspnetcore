using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;

namespace Auth0.AspNetCore.Mvc.TagHelpers.Lock.OAuth
{
    [HtmlTargetElement("lock-oauth-configuration", ParentTag = "lock")]
    public class LockOAuthConfigurationTagHelper : TagHelper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string AuthenticationScheme { get; set; }

        public LockOAuthConfigurationTagHelper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            //_httpContextAccessor = httpContextAccessor;
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
                //lockContext.CallbackUrl = BuildRedirectUri(_httpContextAccessor.HttpContext.Request,
                //    options.CallbackPath);
            }

            output.SuppressOutput();

            return Task.FromResult(0);
        }

        protected string BuildRedirectUri(HttpRequest httpRequest, PathString redirectPath)
        {
            return httpRequest.Scheme + "://" + httpRequest.Host + redirectPath;
        }
    }
}