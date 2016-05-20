public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    loggerFactory.AddConsole(Configuration.GetSection("Logging"));
    loggerFactory.AddDebug();

    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseBrowserLink();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
    }

    app.UseStaticFiles();
    
    // Add the cookie middleware
    app.UseCookieAuthentication(new CookieAuthenticationOptions
    {
        AutomaticAuthenticate = true,
        AutomaticChallenge = true
    });
    
    // Add the OAuth middleware
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
    });
}