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
    
    Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            // Retrieve user info
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            // Extract the user info object
            var user = JObject.Parse(await response.Content.ReadAsStringAsync());

            // Add the Name Identifier claim
            var userId = user.Value<string>("user_id");
            if (!string.IsNullOrEmpty(userId))
            {
                context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId, ClaimValueTypes.String, context.Options.ClaimsIssuer));
            }

            // Add the Name claim
            var email = user.Value<string>("email");
            if (!string.IsNullOrEmpty(email))
            {
                context.Identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, email, ClaimValueTypes.String, context.Options.ClaimsIssuer));
            }
        }
    }
});
