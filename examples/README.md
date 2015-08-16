# Auth0 ASP.NET 5 Examples (beta6)

## WebApi-RS256-Sample

This sample shows how to use Auth0 for authentication with your ASP.NET Web Api using RS256 (JWT signed with your token signing key in Auth0).

In order to run this sample, you must first configure your application in Auth0 as follows:

- Go to https://manage.auth0.com/#/applications/{YOUR_AUTH0_CLIENT_ID}/settings
- Click on Show Advanced Settings button.
- Set RS256 as JsonWebToken Token Signature Algorithm and click on Save.

Then once you got a token from Auth0, use it to call your API by adding it to the `Authorization` header:

```
Authorization: Bearer <token>
```

*Tip:* You can use [the Resource Owner endpoint](https://auth0.com/docs/auth-api#!#post--oauth-ro) to easily generate tokens.

## WebApp-OpenIdConnect-Sample

This sample shows how to use Auth0 for authentication with your ASP.NET MVC application.

In order to run this sample, you must first configure your application in Auth0 as follows:

- Go to https://manage.auth0.com/#/applications/{YOUR_AUTH0_CLIENT_ID}/settings
- Set the `Allowed Callback URLs` to **http://localhost:5001**
- Click on Show Advanced Settings button.
- Set RS256 as JsonWebToken Token Signature Algorithm and click on Save.

Make sure you also restore the Bower and NPM dependencies before starting the project.
