using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Auth0.AspNetCore.Mvc.TagHelpers
{
    [HtmlTargetElement("lock")]
    public class LockTagHelper : TagHelper
    {
        private string callbackUrl;
        private string clientId;
        private string domain;

        public LockMode Mode { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Create context which child configuration tag helpers will use to set correct parameters for Lock
            var lockContext = new LockContext();
            context.Items.Add(typeof(LockContext), lockContext);

            // Allow child tag helpers to set correct configuration parameters
            await output.GetChildContentAsync();

            string modalTemplate =
                $@"
            <script src=""https://cdn.auth0.com/js/lock-9.1.min.js""></script>
            <script type=""text/javascript"">
  
                var lock = new Auth0Lock('{lockContext.ClientId}', '{lockContext.Domain}');
  
                function signin() {{
                    lock.show({{
                        callbackURL: '{lockContext.CallbackUrl}'
                        , responseType: 'code'
                        , authParams: {{
                            scope: 'openid email'
                            , state: '{lockContext.State}'
                            , nonce: '{lockContext.Nonce}'
                        }}
                    }});
                }}
            </script>

            <button onclick=""window.signin();"">Login</button>
            ";

            string inlineTemplate =
                $@"
                <div id=""root"" style=""width: 320px; margin: 40px auto; padding: 10px; border-style: dashed; border-width: 1px;"">
                </div>            
                <script src=""https://cdn.auth0.com/js/lock-9.1.min.js""></script>
                <script>
  
                    var lock = new Auth0Lock('{lockContext.ClientId}', '{lockContext.Domain}');
  
                    lock.show({{
                        container: 'root'
                    , callbackURL: '{lockContext.CallbackUrl}'
                    , responseType: 'code'
                    , authParams: {{
                        scope: 'openid email'  // Learn about scopes: https://auth0.com/docs/scopes 
                        , state: '{lockContext.State}'
                            , nonce: '{lockContext.Nonce}'
                    }}
                    }});
                </script>
                ";


            output.TagName = "div";

            if (Mode == LockMode.Inline)
            {
                output.Content.AppendHtml(inlineTemplate);
            }
            else
            {
                output.Content.AppendHtml(modalTemplate);
            }
        }
    }
}