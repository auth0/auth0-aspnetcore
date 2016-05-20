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
    
    // Add the OIDC middleware
    app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions("Auth0")
    {
        ClaimsIssuer = "Auth0"

        Authority = "https://YOUR_AUTH0_DOMAIN",
        ClientId = "APP_CLIENT_ID",
        ClientSecret = "APP_CLIENT_SECRET",

        AutomaticAuthenticate = true, 
        AutomaticChallenge = true,

        ResponseType = "code",

        CallbackPath = new PathString("/signin-auth0")
    });
}