using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Auth0.AspNetCore.Mvc.TagHelpers
{
    [HtmlTargetElement("lock-static-configuration", ParentTag = "lock")]
    public class LockStaticConfigurationTagHelper : TagHelper
    {
        public string CallbackUrl { get; set; }

        public string ClientId { get; set; }

        public string Domain { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var lockContext = (LockContext) context.Items[typeof(LockContext)];
            lockContext.CallbackUrl = CallbackUrl;
            lockContext.ClientId = ClientId;
            lockContext.Domain = Domain;

            return Task.FromResult(0);
        }
    }
}