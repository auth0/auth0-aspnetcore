---
uid: mvc-oauth2
---

# Using OAuth2

Auth0 is built on open standards which means that you can use the standard ASP.NET Core middleware, such as the OAuth2 middleware, to sign users into your application. This document will guide you through configuring the OAuth2 middleware to work with Auth0.   

## Basic configuration

Let us first cover the most basic configuration you need, and then we will cover more advanced configuration scenarios later in this document.

### 1. Configure your Auth0 application

Go to the [Auth0 Dashboard](https://manage.auth0.com) and add the URL `http://<YOUR_APPLICATION_URL>/signin-auth0` to your list of callback URLs.

### 2. Add the cookie and OAuth NuGet packages

Next you will need to add the Cookie middleware and OAuth middleware you your project:

```
Install-Package Microsoft.AspNetCore.Authentication.Cookies
Install-Package Microsoft.AspNetCore.Authentication.OAuth
```

### 3. Configure Authentication Services

In the `ConfigureServices` of your `Startup` class, ensure that you add the authentication services and configure cookie authentication as the default authentication scheme: 

[!code-csharp[Startup](snippets/oauth-services.cs?highlight=4-5)]

### 4. Configure the cookie and OAuth middleware

In the `Configure` method of your `Startup` class, register the Cookie middleware:

[!code-csharp[Startup](snippets/oauth-cookie-basic.cs?highlight=19-23)]

And also register the OAuth middleware:

[!code-csharp[Startup](snippets/oauth-oauth-basic.cs?highlight=26-40)]

## Advanced configuration

### Using login and logout routes

In the basic configuration settings we have configured the OAuth middleware to automatically challenge when a user wants to access a protected resource. You may however also want to configure a Login and Logout route which you can redirect a user to.

#### 1. Configure the Cookie middleware to redirect to routes

The first step in this configuration is to alter the cookie middleware to redirect to a login and logout route when a user was not successfully authenticated by the cookie middleware:

[!code-csharp[Startup](snippets/oauth-cookie-routes.cs?highlight=5-6)]  

#### 2. Do not automatically challenge in OAuth middleware

The second step is that the OAuth middleware should not automatically challenge and login:

[!code-csharp[Startup](snippets/oauth-oauth-routes.cs?highlight=5-6)]  

#### 3. Handle login and logout routes

The last step is to actually handle the login and logout routes. After registration of the OAuth middleware you can register middleware to handle the `/login` path. This middleware should return a challenge response which will invoke the OAuth middleware:

[!code-csharp[Startup](snippets/oauth-login.cs)]

And you will also need to register middleware to handle the `/logout` path. This will simply sign the user out of the authentication middleware:

[!code-csharp[Startup](snippets/oauth-logout.cs)]

### Getting the User Profile

The OAuth middleware does not automatically request the user profile, even though you specify `UserInformationEndpoint` property when registering the OAuth middleware. You will need to handle the `OnCreatingTicket` event to manually request the user information from the endpoint, extract the JSON payload containing the user information and set the relevant claims:

[!code-csharp[Startup](snippets/oauth-oauth-userprofile.cs?highlight=21-47)] 

### Storing the tokens as claims

You may also want to add the tokens which was received by the OAuth middleware as claims. In order to achieve this you will need to set the `SaveTokens` property to true. This will save the tokens as properties which you can then retrieve in the `OnTicketReceived` event. 

Then simply locate the property called `.TokenNames` which will contain the names of all the token properties which was saved as a semi-colon separated list. The retrieve the values of the properties for each of those token names and save them as claims:

[!code-csharp[Startup](snippets/oauth-oauth-tokenclaims.cs?highlight=19,23-43)] 
  