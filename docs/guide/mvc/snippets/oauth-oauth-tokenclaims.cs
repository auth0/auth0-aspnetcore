app.UseOAuthAuthentication(new OAuthOptions
{
    AuthenticationScheme = "Auth0",

    AutomaticAuthenticate = true,
    AutomaticChallenge = true

    ClientId = "APP_CLIENT_ID",
    ClientSecret = "APP_CLIENT_SECRET",

    CallbackPath = new PathString("/signin-auth0"),

    AuthorizationEndpoint = "https://YOUR_AUTH0_DOMAIN/authorize",
    TokenEndpoint = "https://YOUR_AUTH0_DOMAIN/oauth/token",
    UserInformationEndpoint = "https://YOUR_AUTH0_DOMAIN/userinfo",

    Scope = { "openid" },
    
    SaveTokens = true,
    
    Events = new OAuthEvents
    {
        OnTicketReceived = context =>
        {
            var identity = context.Principal.Identity as ClaimsIdentity;
            if (identity != null)
            {
                if (context.Properties.Items.ContainsKey(".TokenNames"))
                {
                    string[] tokenNames = context.Properties.Items[".TokenNames"].Split(';');
                    
                    foreach(string tokenName in tokenNames)
                    {
                        string tokenValue = context.Properties.Items[$".Token.{tokenName}"];
                        
                        if (!identity.HasClaim(c => c.Type == tokenName))
                            identity.AddClaim(new Claim(tokenName, tokenValue));
                    }
                }
            }

            return Task.FromResult(0);
        }
    }
});
