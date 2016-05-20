---
uid: mvc-openid-connect
---

Auth0 is built on open standards which means that you can use the standard ASP.NET Core middleware, such as the OIDC middleware, to sign users into your application.

This document will guide you through configuring the OIDC middleware to work with Auth0.   

## Basic configuration

Let us first cover the most basic configuration you need, and then we will cover more advanced configuration scenarios later in this document.

### 1. Configure your Auth0 application

Go to the [Auth0 Dashboard](https://manage.auth0.com) and esure:

* You add the URL `http://<YOUR_APPLICATION_URL>/signin-auth0` to your list of callback URLs
* You configure your application to sign JWT using RS256

> At this moment the OIDC middleware does not support JSON Web Tokens signed with HS256, so it is important to use RS256

### 2. Add the cookie and OIDC NuGet packages

Next you will need to add the Cookie middleware and OIDC middleware you your project:

```
Install-Package Microsoft.AspNetCore.Authentication.Cookies
Install-Package Microsoft.AspNetCore.Authentication.OpenIdConnect
```

### 3. Configure Authentication Services

In the `ConfigureServices` of your `Startup` class, ensure that you add the authentication services and configure cookie authentication as the default authentication scheme: 

[!code-csharp[Startup](snippets/oidc-services.cs?highlight=4-5)]

### 4. Configure the cookie and OIDC middleware

In the `Configure` method of your `Startup` class, register the Cookie middleware:

[!code-csharp[Startup](snippets/oidc-cookie-basic.cs?highlight=19-23)]

And also register the OIDC middleware:

[!code-csharp[Startup](snippets/oidc-oidc-basic.cs?highlight=26-40)]

## Advanced configuration

### Using login and logout routes

In the basic configuration settings we have configured the OIDC middleware to automatically challenge when a user wants to access a protected resource. You may however also want to configure a Login and Logout route which you can redirect a user to.

#### 1. Configure the Cookie middleware to redirect to routes

The first step in this configuration is to alter the cookie middleware to redirect to a login and logout route when a user was not successfully authenticated by the cookie middleware:

[!code-csharp[Startup](snippets/oidc-cookie-routes.cs?highlight=5-6)]  

#### 2. Do not automatically challenge in OIDC middleware

The second step is that the OIDC middleware should not automatically challenge and login:

[!code-csharp[Startup](snippets/oidc-oidc-routes.cs?highlight=9-10)]  

#### 3. Handle login and logout routes

The last step is to actually handle the login and logout routes. After registration of the OIDC middleware you can register middleware to handle the `/login` path. This middleware should return a challege response which will invoke the OIDC middleware:

[!code-csharp[Startup](snippets/oidc-login.cs)]

And you will also need to register middleware to handle the `/logout` path. This will simply sign the user out of the authentication middleware:

[!code-csharp[Startup](snippets/oidc-logout.cs)]

### Setting the name claim

Another important configuration option which you may want to do is to add a Claim with the user's name. This is important when you want to display the user's name somewhere in the user interface by using the `User.Identity.Name` property.

To achieve this you will need to handle the `OnTicketReceived` event in the OIDC middleware. The OIDC middleware will already set a Claim with the type of `name` which is the name of the user. You will simply need to retrieve that value and set it as the value for the Claim with the claim type of `ClaimTypes.Name`:

[!code-csharp[Startup](snippets/oidc-oidc-nameclaim.cs?highlight=43-55)] 

### Storing the tokens as claims

You may also want to add the tokens which was received by the OIDC middleware as claims. In order to achieve this you will need to set the `SaveTokens` property to true. This will save the tokens as properties which you can then retrieve in the `OnTicketReceived` event. 

Then simply locate the property called `.TokenNames` which will contain the names of all the token properties which was saved as a semi-colon separated list. The retrieve the values of the properties for each of those token names and save them as claims:

[!code-csharp[Startup](snippets/oidc-oidc-tokenclaims.cs?highlight=41,50-61)] 
  