---
title: On-behalf-of flows with MSAL.NET
description: "How to use MSAL.NET to authenticate on behalf of a user."
---

# On-behalf-of flows with MSAL.NET

## If you are using ASP.NET Core or ASP.NET Classic

If you are building a web API on top of ASP.NET Core or ASP.NET Classic, we recommend that you use `Microsoft.Identity.Web`. See [Web APIs with `Microsoft.Identity.Web`](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apis).

Check the decision tree: [Is MSAL.NET right for me?](/entra/msal/dotnet/getting-started/choosing-msal-dotnet)

## Getting tokens on behalf of a user

### Scenario

- A client (website, desktop, mobile, single-page application) - not represented in the picture below - calls a protected web API, providing a JWT bearer token in its "Authorization" HTTP header.
- The protected web API validates the incoming user token and uses MSAL.NET `AcquireTokenOnBehalfOf` method to request from Microsoft Entra another token so that it can, itself, call another web API, for example, Graph, named the downstream web API, on behalf of the user.

This flow, named the On-Behalf-Of flow (OBO), is illustrated by the top part of the picture below. The bottom part is a daemon scenario, also possible for web APIs.

![image](../../media/on-behalf-flow.png)

### How to call OBO

The OBO call is done by calling the <xref:Microsoft.Identity.Client.IConfidentialClientApplication.AcquireTokenOnBehalfOf(System.Collections.Generic.IEnumerable{System.String},Microsoft.Identity.Client.UserAssertion)> method on the `IConfidentialClientApplication` interface.

This call looks in the cache by itself - so you do not need to call `AcquireTokenSilent`, and it does not store refresh tokens.

For scenarios where continuous access is needed without an assertion, see [OBO for long lived processes](../web-apps-apis/on-behalf-of-flow.md)

> **Note:** Make sure to pass an access token, not an ID token, into the `AcquireTokenOnBehalfOf` method. The purpose of an ID token is a confirmation that a user was authenticated, and it contains some user-related information. In contrast, an access token determines whether a user has access to a resource, which is more appropriate in this On-Behalf-Of scenario. MSAL is focused on getting good access tokens. ID tokens are also obtained and cached but their expiry is not tracked. So an ID token can expire and `AcquireTokenSilent` will not refresh it.

```csharp
private async Task AddAccountToCacheFromJwt(IEnumerable<string> scopes, JwtSecurityToken jwtToken, ClaimsPrincipal principal, HttpContext httpContext)
{
  if (jwtToken == null)
  {
    throw new ArgumentOutOfRangeException("tokenValidationContext.SecurityToken should be a JWT Token");
  }
  UserAssertion userAssertion = new UserAssertion(jwtToken.RawData, "urn:ietf:params:oauth:grant-type:jwt-bearer");
  IEnumerable<string> requestedScopes = scopes ?? jwtToken.Audiences.Select(a => $"{a}/.default");

  // Create the application
  var application = BuildConfidentialClientApplication(httpContext, principal);

  // await to make sure that the cache is filled in before the controller tries to get access tokens
  var result = await application.AcquireTokenOnBehalfOf(requestedScopes, userAssertion).ExecuteAsync();                     
}
```

## Handling multi-factor auth (MFA), conditional access and incremental consent


### Failure scenario

It is a common scenario that a tenant administrator restricts access to the downstream API (for example, to Graph) by requiring end-users to complete a Multi-Factor Authentication (MFA) challenge; however, they often do not place the same restrictions on the web API. 

1. The client (e.g., desktop app or website) asks for a token for your web API. MFA is not enforced at this point.
2. The web API tries to exchange this token for a token for the downstream web API (e.g. Graph) via the on-behalf-of flow. This fails because access through Graph requires the user to have completed the MFA challenge. The call to `AcquireTokenOnBehalfOf` will fail with an `MsalUiRequiredException` which will also have the `Claims` property set.

### How to signal that MFA is needed to the client

The web API needs to [send the exception back to the client](/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow#error-response-example) with the claims string. 
The [standard pattern](https://datatracker.ietf.org/doc/html/rfc6750#section-3.1) for signaling this failure to a client is to reply with `HTTP 401` and with a `WWW-Authenticate` header which encapsulates the details of the failure. 


#### The web API replies with 401 + WWW-Authenticate

```csharp
// This example is for an ASP.NET Core web API
public void ReplyUnauthorizedWithWwwAuthenticateHeader(MsalUiRequiredException ex)
{
     httpResponse.StatusCode = (int)HttpStatusCode.Unauthorized; // HTTP 401
     httpResponse.Headers[HeaderNames.WWWAuthenticate] = $"Bearer claims={ex.Claims}, error={ex.Message};";
}
```

#### Handling the failure on the client 

The client needs to interpret `401` messages and parse `WWW-Authenticate` headers. MSAL.NET offers parsing APIs: 

```csharp
// assuming an HttpResponseMessage response with StatusCode=HttpStatusCode.Unauthorized
WwwAuthenticateParameters wwwParams = WwwAuthenticateParameters.CreateFromAuthenticationHeaders(response.HttpResponseHeaders, "Bearer");
string claims = wwwParams.Claims; // you may also extract other parameters such as Error and Authority

// desktop or mobile app
app.AcquireTokenInteractive(scopes).WithClaims(wwwParams.Claims);

// web app - redirect to the login page and add the claims to the authorization URL
RedirectToLogin(wwwParams.ConsentUri);
```

## Long-running OBO processes

One OBO scenario is when a web API runs long-running processes on behalf of the user (for example, OneDrive which creates albums for you). This can be implemented as such:

1. Before you start a long-running process, call:

```csharp
string sessionKey = // custom key or null
var authResult = await ((ILongRunningWebApi)confidentialClientApp)
         .InitiateLongRunningProcessInWebApi(
              scopes,
              userAccessToken,
              ref sessionKey)
         .ExecuteAsync();
```

`userAccessToken` is a user access token used to call this web API. `sessionKey` will be used as a key when caching and retrieving the OBO token. If set to `null`, MSAL will set it to the assertion hash of the passed-in user token. It can also be set by the developer to something that identifies a specific user session, like the optional `sid` claim from the user token (for more information, see [Provide optional claims to your app](/azure/active-directory/develop/active-directory-optional-claims)). `InitiateLongRunningProcessInWebApi` doesn't check the cache; it will use the user token to acquire a new OBO token from Microsoft Entra ID, which will then be cached and returned.

2. In the long-running process, whenever OBO token is needed, call `AcquireTokenInLongRunningProcess` in the following pattern:

```csharp
try {  
    authResult = await ((ILongRunningWebApi)confidentialClientApp)  
         .AcquireTokenInLongRunningProcess(  
              scopes,  
              sessionKey)  
         .ExecuteAsync();  
}
catch (MsalClientException ex) {  
    // No tokens were found with this cache key.  
    // First call InitiateLongRunningProcessInWebApi with a valid user assertion
    // to acquire tokens from Microsoft Entra ID and cache them.
    if (ex.ErrorCode == MsalError.OboCacheKeyNotInCacheError)
    {
          authResult = await ((ILongRunningWebApi)confidentialClientApp)
         .InitiateLongRunningProcessInWebApi(
              scopes,
              userAccessToken, //Valid access token
              ref sessionKey)
         .ExecuteAsync();
    }

} catch (MsalUiRequiredException ex) {  
    // A refresh token was used to acquire new tokens  
    // but Microsoft Entra ID requires the user to sign in again.  
    // Trigger your app's user sign-in again by replying with a 401 + WWW-Authenticate  
    // Then call InitiateLongRunningProcessInWebApi once a new access token is acquired from the user
     httpResponse.StatusCode = (int)HttpStatusCode.Forbidden;
     httpResponse.Headers[HeaderNames.WWWAuthenticate] = $"Bearer claims={ex.Claims}, error={ex.Message};
}
```

Pass the `sessionKey` which is associated with the current user's session and will be used to retrieve the related OBO token. If the token is expired, MSAL will use the cached refresh token to acquire a new OBO access token from Microsoft Entra ID and cache it. If no token is found with this `sessionKey`, MSAL will throw an `MsalClientException` or a `MsalUiRequiredException`. Make sure to acquire a valid user token and call `InitiateLongRunningProcessInWebApi` if this is the case.

### Cache eviction for long-running OBO processes

It is strongly recommended to [use a distributed persisted cache](/azure/active-directory/develop/msal-net-token-cache-serialization?tabs=aspnetcore) in a web API scenario. Since these APIs store the refresh token, MSAL will not suggest an expiration, as refresh tokens have a long lifetime and can be used over and over again.

It is recommended that you set L1 and L2 eviction policies manually, for example, a max size for the L1 cache and a sliding expiration for the L2.

### Exception handling

In a case when `AcquireTokenInLongRunningProcess` throws an exception when it cannot find a token and the L2 cache has a cache entry for the same cache key, verify that the L2 cache read operation is completed successfully. `AcquireTokenInLongRunningProcess` is different from the `InitiateLongRunningProcessInWebApi` and `AcquireTokenOnBehalfOf`, in that it is if the cache read fails, this method is unable to acquire a new token from Microsoft Entra ID because it does not have an original user assertion. If using Microsoft.Identity.Web.TokenCache to enable distributed cache, set [OnL2CacheFailure](https://github.com/AzureAD/microsoft-identity-web/wiki/Token-Cache-Troubleshooting#i-configured-a-distributed-l2-cache-but-nothings-gets-written-to-it) event to retry the L2 call and/or add extra logs, which can be enabled [through built-in MSAL functionality](../../advanced/exceptions/msal-logging.md).

### Removing accounts

Starting with MSAL 4.51.0, to remove cached tokens call `StopLongRunningProcessInWebApiAsync` passing in a cache key. With earlier MSAL versions, it is recommended to use L2 cache eviction policies. If immediate removal is needed, delete the L2 cache node associated with the `sessionKey`.

### Troubleshooting

If you are updating MSAL.NET to 4.51.0+, there is a chance that `InitiateLongRunningProcessInWebApi` will stop returning tokens and throw an exception if you are relying upon it to return tokens to you after the long-running process is already initiated and there is a token in the cache for the specified cache key. `InitiateLongRunningProcessInWebApi` no longer inspects the cache to acquire tokens. Please use `AcquireTokenInLongRunningProcess` to continue to access the currently active long-running process.  The `InitiateLongRunningProcessInWebApi` should only be used to initiate the process. If it is not possible to make these changes quickly, and you are updating to MSAL 4.54.1 or higher, you can use `InitiateLongRunningProcessInWebApi().WithSearchInCacheForLongRunningProcess()` to revert the behavior of `InitiateLongRunningProcessInWebApi`

## App registration - specificities for Web APIs

- Web APIs expose scopes. For more information, see [Quickstart: Configure an application to expose web APIs (Preview)](/azure/active-directory/develop/quickstart-configure-app-expose-web-apis).

- Web APIs decide which version of the token they want to accept. For your own web API, you can change the property in the manifest named `accessTokenAcceptedVersion` (to 1 or 2). For more information, see [Microsoft Entra app manifest](/azure/active-directory/develop/reference-app-manifest).

## Practical usage of OBO in an ASP.NET / ASP.NET Core application

If you are building a web API on top of ASP.NET Core, we recommend that you use `Microsoft.Identity.Web`. See [Web APIs with `Microsoft.Identity.Web`](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apis).

In an ASP.NET / ASP.NET Core Web API, OBO is typically called on the `OnTokenValidated` event of the `JwtBearerOptions`. The token is then not used immediately, but this call has the effect of populating the user token cache. Later, the controllers will call `AcquireTokenSilent`, which will have the effect of hitting the cache, refreshing the access token if needed, or getting a new one for a new resource, but still for the same user.

Here is what happens when a JWT bearer token is received and validated by the Web API:

```csharp
public static IServiceCollection AddProtectedApiCallsWebApis(this IServiceCollection services, IConfiguration configuration, IEnumerable<string> scopes)
{
 ...
 services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
 {
  options.Events.OnTokenValidated = async context =>
  {
   var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
   context.Success();

   // Adds the token to the cache, and also handles the incremental consent and claim challenges
   tokenAcquisition.AddAccountToCacheFromJwt(context, scopes);
   await Task.FromResult(0);
  };
 });
 return services;
}
```

And here is the code in the actions of the API controllers, calling downstream APIs:

```csharp
private async Task GetTodoList(bool isAppStarting)
{
 ...
 //
 // Get an access token to call the To Do service.
 //
 AuthenticationResult result = null;
 try
 {
  result = await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                     .ExecuteAsync()
                     .ConfigureAwait(false);
 }
...

// Once the token has been returned by MSAL, add it to the http authorization header, before making the call to access the To Do list service.
// Make sure to use an access token and not an ID token
_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

// Call the To Do list service.
HttpResponseMessage response = await _httpClient.GetAsync(TodoListBaseAddress + "/api/todolist");
...
}
```

the GetAccountIdentifier method uses the claims associated with the identity of the user for which the Web API received the JWT:

```csharp
public static string GetMsalAccountId(this ClaimsPrincipal claimsPrincipal)
{
 string userObjectId = GetObjectId(claimsPrincipal);
 string tenantId = GetTenantId(claimsPrincipal);

 if (!string.IsNullOrWhiteSpace(userObjectId) && !string.IsNullOrWhiteSpace(tenantId))
 {
  return $"{userObjectId}.{tenantId}";
 }

 return null;
}
```

## Protocol

For more information about the On-Behalf-Of protocol, see [Azure Active Directory v2.0 and OAuth 2.0 On-Behalf-Of flow](/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow).

## Samples illustrating the on-behalf of flow

Sample | Platform | Description
------ | -------- | -----------
[active-directory-aspnetcore-webapi-tutorial-v2](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/tree/master/2.%20Web%20API%20now%20calls%20Microsoft%20Graph) | ASP.NET Core 2.2 Web API, Desktop (WPF) | ASP.NET Core 2.1 Web API calling Microsoft Graph, itself called from a WPF application using Azure AD v2 ![On-behalf-of flow topology](../../media/obo-flow-topology.png)
