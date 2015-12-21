# Auth0 ASP.NET 5 Examples (RC1)

## WebApi-RS256-Sample

This sample shows how to use Auth0 for authentication with your ASP.NET Web Api using RS256 (JWT signed with your token signing key in Auth0).

In order to run this sample, you must first configure your application in Auth0 as follows:

- Go to https://manage.auth0.com/#/applications/{YOUR_AUTH0_CLIENT_ID}/settings
- Click on Show Advanced Settings button.
- Set RS256 as JsonWebToken Token Signature Algorithm and click on Save.

Then update the `config.json` file with your account information.

Then once you got a token from Auth0, use it to call your API by adding it to the `Authorization` header:

```
Authorization: Bearer <token>
```

*Tip:* You can use [the Resource Owner endpoint](https://auth0.com/docs/auth-api#!#post--oauth-ro) to easily generate tokens.

## WebApp-OpenIdConnect-Sample

This sample shows how to use Auth0 for authentication with your ASP.NET MVC application.

In order to run this sample, you must first configure your application in Auth0 as follows:

- Go to https://manage.auth0.com/#/applications/{YOUR_AUTH0_CLIENT_ID}/settings
- Set the `Allowed Callback URLs` to **http://localhost:5001/signin-oidc**
- Click on Show Advanced Settings button.
- Set RS256 as JsonWebToken Token Signature Algorithm and click on Save.

Then update the `config.json` file with your account information.

Make sure you also restore the Bower and NPM dependencies before starting the project.

### Using Refresh Tokens

If you need a refresh token you can start by requesting it when signing in (the `offline_access` scope and the `device` parameter):

```
lock.show({
    closable: false,
    callbackURL: window.location.origin + '/signin-oidc',
    authParams: {
        nonce: '@Model.Nonce',
        state: '@Model.State',
        device: 'My Web App',
        scope: 'openid nickname email offline_access',
        response_type: 'code'
    }
});
```

Then when you initialize Auth0 you can store the `refresh_token` in the user object:

```
services.UseAuth0(settings.Domain, settings.ClientId, settings.ClientSecret, settings.RedirectUri, notification =>
{
    var identity = notification.AuthenticationTicket.Principal.Identity as ClaimsIdentity;
    if (!String.IsNullOrEmpty(notification.TokenEndpointResponse.RefreshToken))
        identity.AddClaim(new Claim("refresh_token", notification.TokenEndpointResponse.RefreshToken)); 
    return Task.FromResult(true);
});
```

And now from anywhere in your application you can get the user's refresh token and use it:

```
var refreshTokenClaim = User.FindFirst("refresh_token");
```

### API Controllers

If you're hosting Web APIs within your ASP.NET MVC Web Application you don't need anything special for authenticated endpoints to work. When a user logs in the authentication scheme will be Cookies, so by reusing that same scheme in your API Controllers your authenticated actions will just work:

```
[HttpGet]
[Authorize(ActiveAuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
[Route("api/secured/ping")]
public object SecuredPing()
{
    return new
    {
        message = "Pong. You accessed a protected endpoint.",
        claims = User.Claims.Select(c => new { c.Type, c.Value })
    };
}
```

## Running the samples from the command line

Using powershell, run the following from the project folder:

```
 powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"
 dnvm use 1.0.0-beta8
 dnu restore
 dnx web
```