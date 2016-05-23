using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Auth0.AspNetCore.Mvc.TagHelpers
{
    [HtmlTargetElement("lock")]
    public class LockTagHelper : TagHelper
    {

        public LockMode Mode { get; set; }

        public string CallbackUrl { get; set; }

        public string Domain { get; set; }

        public string ClientId { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string modalTemplate =
                $@"
            <script src=""https://cdn.auth0.com/js/lock-9.1.min.js""></script>
            <script type=""text/javascript"">
  
                var lock = new Auth0Lock('{ClientId}', '{Domain}');
  
                function signin() {{
                    lock.show({{
                        callbackURL: '{CallbackUrl}'
                        , responseType: 'code'
                        , authParams: {{
                        scope: 'openid email'
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
  
                    var lock = new Auth0Lock('{ClientId}', '{Domain}');
  
                    lock.show({{
                        container: 'root'
                    , callbackURL: '{CallbackUrl}'
                    , responseType: 'code'
                    , authParams: {{
                        scope: 'openid email'  // Learn about scopes: https://auth0.com/docs/scopes 
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