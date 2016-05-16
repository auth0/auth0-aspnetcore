---
uid: mvc-extensions
---

# Using the Auth0 extension methods

## Introduction

The easiest way to get started with securing your application with Auth0 is to make use of our extension methods. This is ideal insituations where you are not using any other authentication middleware (such as ASP.NET Indentity) and want to simply allow users to sign using Auth0.

## Installation

To use the extension methods, you will need to install the `Auth0.AspNetCore.Authentication` package:

```
Install-Package Auth0.AspNetCore.Authentication
```

## Configuring Services



## Configuring Middleware



## Understanding what the extension methods does

Using the extension methods makes it easy to get started with integrating Auth0 into your ASP.NET Core application, but it is important to understand what the extension methods does, especially if you are also integrating other security middleware in your middleware pipeline.

For scenarios where you have other security middleware configured, you may want to configure the OpenID Connect or OAuth2 middleware manually, as this will give you more fine-grained control.

The Auth0 middleware consists of 2 methods:

* The `UseAuth0()` method registers the cookie middleware with the Dependency Injection as this is required to store any user credentials received from the OpenID Connect middleware.
* The `AddAuth0()` method will add the Cookie middleware and OpenID Connect middleware to the ASP.NET Core middleware pipeline

The process for authenticating requests will therefor be as follows:

* When a request comes in which requires an authenticated user, the cookie middleware will execute to see whether the user has already been authenticated by determining if an authentication cookie exists.
* If the cookie middleware is unable to authenticate a user, the request will be passed on to the OpenID Connect middlware which will redirect to the Auth0 website where the user will be promted to sign in.
* Once the user has signed in successfully they will be redirected back to your ASP.NET Core website and an authentication cookie will be saved, so subsequent requests will be autenticated by the cookie middleware.

The extensions methods allow for limited configuration, so for any authentication pipeline which differs significantly from this scenario, we suggest that you manually configure the authentication pipeline using either the [OpenID Connect middlware](xref:mvc-openid-connect), [OAuth2 middleware](xref:mvc-oauth2) or the [Authentication API](xref:mvc-auth-api).