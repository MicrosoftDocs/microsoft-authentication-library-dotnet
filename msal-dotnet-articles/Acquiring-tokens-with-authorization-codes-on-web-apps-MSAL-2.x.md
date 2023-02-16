> This page is for MSAL 2.x
> 
> If you are interested in MSAL 3.x, please see [Acquiring tokens with authorization codes on web apps](Acquiring tokens with authorization codes on web apps)

### Getting tokens by authorization code (Web Sites)
When users login to Web applications (web sites) using Open Id connect, the web application receives an authorization code which it can redeem to acquire a token for Web APIs.

### Getting tokens by authorization code in MSAL.NET

To redeem an authorization code and get a token, and cache it, you'll call [AcquireTokenByAuthorizationCodeAsync](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.confidentialclientapplication.acquiretokenbyauthorizationcodeasync?view=azure-dotnet)

![image](https://user-images.githubusercontent.com/13203188/37082327-3716113e-21e4-11e8-921a-31f052ccdd84.png)

The principle is exactly the same for MSAL.NET as [for ADAL.NET](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps), and is illustrated in the active-directory-dotnet-webapp-openidconnect-v2 sample, in [Startup.Auth.cs, Lines 70 to 87](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2/blob/master/WebApp/App_Start/Startup.Auth.cs#L76-L93). ASP.NET triggers an authentication code flow because the scopes [App_Start/Startup.Auth.cs#L53](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2/blob/c2087374e849fd58b5bf75ffebef1ac0e106884d/WebApp/App_Start/Startup.Auth.cs#L53) contains `open_id`

```CSharp
Scope = "openid profile offline_access Mail.Read Mail.Send",
```
and the application subscribes to the notification when the authorization code get received [App_Start/Startup.Auth.cs#L67-L72](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2/blob/c2087374e849fd58b5bf75ffebef1ac0e106884d/WebApp/App_Start/Startup.Auth.cs#L67-L72)

```CSharp
Notifications = new OpenIdConnectAuthenticationNotifications
{
 AuthorizationCodeReceived = OnAuthorization,
 AuthenticationFailed = OnAuthenticationFailed
}
```

When this notification is processed it acquires a token from the authorization code by calling `AcquireTokenByAuthorizationCodeAsync`.

```CSharp
private async Task OnAuthorization(AuthorizationCodeReceivedNotification context)
{
 var code = context.Code;
 string signedInUserID = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
 TokenCache userTokenCache = new MSALSessionCache(signedInUserID,
                                                   context.OwinContext.Environment["System.Web.HttpContextBase"] as HttpContextBase).GetMsalCacheInstance();
 var cca = new ConfidentialClientApplication(clientId, 
                                             redirectUri,
                                             new ClientCredential(appKey), 
                                             userTokenCache, 
                                             null);
 string[] scopes = { "Mail.Read" };
 try
 {
   // As AcquireTokenByAuthorizationCodeAsync is asynchronous we want to tell ASP.NET that
   // we are handing the code even if it's not done yet, so that it does not concurrently
   // call the Token endpoint itself, otherwise you'll get an error message:
   //    'OAuth2 Authorization code was already redeemed' error message
   context.HandleCodeRedemption();
 
  // Redeem the code
  AuthenticationResult result = await cca.AcquireTokenByAuthorizationCodeAsync(code, scopes);

  // Share the ID Token with ASP.NET, but not the Access Token, otherwise ASP.NET 
  // middleware could prevent a further call to AcquireTokenByAuthorizationCodeAsync to
  // really get to AAD in the case of incremental consent (when the Web app requires more scopes)
  context.HandleCodeRedemption(null, result.IdToken);
 }
 catch (Exception eee)
 {

 }
}
```

### Troubleshooting

- The code is usable only once to redeem a token. `AcquireTokenByAuthorizationCodeAsyncshould` should not be called several times with the same authorization code (it's explicitly prohibited by the protocol standard spec). If you redeem the code several times (consciously, or because you are not aware that a framework also does it for you), you'll get an error:
   `'invalid_grant', 'AADSTS70002: Error validating credentials. AADSTS54005: OAuth2 Authorization code was already redeemed, please retry with a new valid code or use an existing refresh token`

- In particular, if you are writing an ASP.NET / ASP.NET Core application, this might happen if you don't tell the ASP.NET/Core framework that you have already redeemed the code. For this you need to call `context.HandleCodeRedemption()` part of the `AuthorizationCodeReceived` event handler

- Finally, avoid sharing the access token with ASP.NET otherwise this might prevent incremental consent happening correctly (for details see issue #[693](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/693))

This very operation has the side effect of adding the token to the token cache, and therefore the controllers that will need a token later will be able to acquire a token silently, as does the SendMail() method of the [HomeController.cs#L55-L76](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2/blob/c2087374e849fd58b5bf75ffebef1ac0e106884d/WebApp/Controllers/HomeController.cs#L56-L76)

### Protocol documentation

For details about the protocol, see [v2.0 Protocols - OAuth 2.0 authorization code flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow)

### Interesting samples using the authorization code flow

Sample | Description
------ | ------------
active-directory-aspnetcore-webapp-openidconnect-v2 in branch [aspnetcore2-2-signInAndCallGraph](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/aspnetcore2-2-signInAndCallGraph) | Web application that handles sign on via the (AAD V2) unified Azure AD and MSA endpoint, so that users can sign in using both their work/school account or Microsoft account. The sample also shows how to use MSAL to obtain a token for invoking the Microsoft Graph, including how to handle incremental consent. ![Topology](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/aspnetcore2-2-signInAndCallGraph/ReadmeFiles/sign-in.png)
[active-directory-dotnet-webapp-openidconnect-v2](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2) | Web application that handles sign on via the (AAD V2) unified Azure AD and MSA endpoint, so that users can sign in using both their work/school account or Microsoft account. The sample also shows how to use MSAL to obtain a token for invoking the Microsoft Graph. ![Topology](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2/blob/master/ReadmeFiles/Topology.png)
[active-directory-dotnet-admin-restricted-scopes-v2](https://github.com/azure-samples/active-directory-dotnet-admin-restricted-scopes-v2) | An ASP.NET MVC application that shows how to use the Azure AD v2.0 endpoint to collect consent for permissions that require administrative consent. ![Topology](https://github.com/Azure-Samples/active-directory-dotnet-admin-restricted-scopes-v2/blob/master/ReadmeFiles/Topology.png)

> Vanity URL: https://aka.ms/msal-net-authorization-code