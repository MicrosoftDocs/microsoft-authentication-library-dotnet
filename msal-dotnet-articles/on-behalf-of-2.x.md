> This page is for MSAL 2.x
> 
> If you are interested in MSAL 3.x, please see [on behalf of](on-behalf-of)

## Getting tokens on behalf of a user (Service to service calls)

### Scenario

- A client (Web, desktop, mobile, Single-page application) - not represented on the picture below - calls a protected Web API, providing a JWT bearer token in its "Authorization" Http Header.
- The protected Web API validates the token, and uses MSAL.NET `AcquireTokenOnBehalfOfAsync` method to request, to Azure AD, another token so that it can, itself, call a second Web API (named the downstream Web API) on behalf of the user.
- The protected Web API uses this token to call a downstream API, it can also later call `AcquireTokenSilentAsync` to request tokens for other downstream APIs (but still on behalf of the same user). `AcquireTokenSilentAsync` refreshes the token when needed.

This flow, named the on-behalf-of flow (OBO), is illustrated by the top part of the picture below. The bottom part is a daemon scenario, also possible for Web APIs.

![image](https://user-images.githubusercontent.com/13203188/44857544-dfe61c80-ac24-11e8-8682-f697d6fe07c6.png)

### How to call OBO

This flow is only available in confidential client flow, and therefore the protected Web API provides client credentials (client secret or certificate) to the [constructor](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.confidentialclientapplication.-ctor?view=azure-dotnet#Microsoft_Identity_Client_ConfidentialClientApplication__ctor_System_String_System_String_Microsoft_Identity_Client_ClientCredential_Microsoft_Identity_Client_TokenCache_Microsoft_Identity_Client_TokenCache_) of `ConfidentialClientApplication` construction.

![image](https://user-images.githubusercontent.com/13203188/37082196-bc727314-21e3-11e8-8f0f-2ac81c9b955c.png)

The OBO call is done by calling one of the [overrides](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.clientapplicationbase.acquiretokensilentasync?view=azure-dotnet) of ``AcquireTokenOnBehalfOfAsync``, which takes a ``UserAssertion`` parameter of type ``UserAssertion``

The `ClientAssertion` is built from the bearer token received by the Web API from its own clients. There are [two constructors](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.clientcredential.-ctor?view=azure-dotnet), one taking a JWT bearer token, and one taking any kind of user assertion (another kind of security token, which type is then specified in an additional parameter named `assertionType`)

![image](https://user-images.githubusercontent.com/13203188/37082180-afc4b708-21e3-11e8-8af8-a6dcbd2dfba8.png)

In practice, the OBO flow is often used to acquire a token for a downstream API, and store it in the MSAL.NET user token cache, so that other parts of the Web API can, later call on of the [overrides](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.clientapplicationbase.acquiretokensilentasync?view=azure-dotnet) of ``AcquireTokenOnSilentAsync`` to call the downstream APIs (which also has the effect of refreshing the tokens if needed):

```CSharp
private void AddAccountToCacheFromJwt(IEnumerable<string> scopes, JwtSecurityToken jwtToken claimsPrincipal user)
{
    // Create the application
    var credential = new ClientCredential(clientSecret); // or certificate
    TokenCache userTokenCache = tokenCacheProvider.GetCache(user);
    var app = new ConfidentialClientApplication(clientId, replyUri, credential, userTokenCache, null);

    // Call the OBO
    UserAssertion userAssertion = new UserAssertion(jwtToken.RawData,
                                                    "urn:ietf:params:oauth:grant-type:jwt-bearer");

    try
    {
     AuthenticationResult result = await application.AcquireTokenOnBehalfOfAsync(scopes, userAssertion);
    }
    catch (MsalUiRequiredException ex)
    {
        // In case an interaction is required, given that Web API don't have UI they need
        // to send back the content to their client
        ReplyForbiddenWithWwwAuthenticateHeader(httpContext, scopes, ex);
    }
    catch (MsalException ex)
    {
        throw;
    }
}
```

Note that it's important, in Confidential Client Applications to have **one user token cache per user**, which is why the user token cache is requested by the call to `tokenCacheProvider.GetCache(user)`.

For details on how to implement a custom token cache serialization in Web apps or Web APIs, see [Token cache for a Web App / Web API](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization#token-cache-for-a-web-app-confidential-client-application)

## Practical usage of OBO in an ASP.NET / ASP.NET Core application

In an ASP.NET / ASP.NET Core Web API, OBO is typically called on the `OnTokenValidated` event of the `JwtBearerOptions`. The token is then not used immediately, but this call has the effect of populating the user token cache. Later, the controllers will call `AcquireTokenSilentAsync`, which will have the effect of hitting the cache, refreshing the access token if needed, or getting a new one for a new resource, but for still for the same user.

Here is what happens when a Jwt bearer token is received end validated by the Web API:

```CSharp
public class Startup
{
  ...
 public void ConfigureServices(IServiceCollection services)
 {
    ...
    services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
    {
        ...
        options.Events = new JwtBearerEvents();
        options.Events.OnTokenValidated = async context =>
        {
            var scopes = new string[] { "user.read" };

            context.Success();
            AddAccountToCacheFromJwt(scopes, context.SecurityToken as JwtSecurityToken, context.Principal);
        }
    }
 }
}
```

And here is the code in the actions of the API controllers, calling downstream APIs:

```CSharp
[Authorize]
[Route("api/[controller]")]
public class TodoListController : Controller
{
 ...
 [HttpGet]
 public string GetInformationByCallingDownstreamAPI()
 {
  string[] scopes = new string[] { "https://myDownstreamApi/.default" };
  try
  {
   string accountIdentifier = GetMsalAccountId(HttpContext.Principal);
   IAccount account = await application.GetAccountAsync(accountIdentifier);
   var result = await application.AcquireTokenSilentAsync(scopes, account);
   return await CallDownstreamApiOnBehalfOfUser(result.AccessToken)
  }
  catch (MsalUiRequiredException ex)
  {
    ...
  }
   ...
 }
 ...
}
```

the GetAccountIdentifier method uses the claims associated with the identity of the user for which the Web API received the JWT:

```CSharp
public static string GetMsalAccountId(ClaimsPrincipal claimsPrincipal)
{
    string userObjectId = claimsPrincipal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
    if (string.IsNullOrEmpty(userObjectId))
    {
        userObjectId = claimsPrincipal.FindFirstValue("oid");
    }
    string tenantId = claimsPrincipal.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");
    if (string.IsNullOrEmpty(tenantId))
    {
        tenantId = claimsPrincipal.FindFirstValue("tid");
    }

    if (string.IsNullOrWhiteSpace(userObjectId))
        throw new ArgumentOutOfRangeException("Missing claim 'http://schemas.microsoft.com/identity/claims/objectidentifier' or 'oid' ");


    if (string.IsNullOrWhiteSpace(tenantId))
        throw new ArgumentOutOfRangeException("Missing claim 'http://schemas.microsoft.com/identity/claims/tenantid' or 'tid' ");

    string accountId = userObjectId + "." + tenantId;
    return accountId;
}
```

## App registration - specificities for Web APIs

- Web APIs expose scopes. For more information, see [Quickstart: Configure an application to expose web APIs (Preview)](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

- Web APIs decide which version of token they want to accept. For your own Web API, you can change the property of the manifest named `acceptedTokenVersion` (to 1 or 2). For more information, see [Azure Active Directory app manifest](https://docs.microsoft.com/en-us/azure/active-directory/develop/reference-app-manifest)

## Protocol

For more information about the on-behalf-of protocol, see [Azure Active Directory v2.0 and OAuth 2.0 On-Behalf-Of flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow)

## Samples illustrating the on-behalf of flow

Sample | Platform | Description
------ | -------- | -----------
[active-directory-aspnetcore-webapi-tutorial-v2](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/tree/master/2.%20Web%20API%20now%20calls%20Microsoft%20Graph) | ASP.NET Core 2.2 Web API, Desktop (WPF) | ASP.NET Core 2.1 Web API calling Microsoft Graph, itself called from a WPF application using Azure AD V2 ![topology](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/blob/master/2.%20Web%20API%20now%20calls%20Microsoft%20Graph/ReadmeFiles/topology.png)

> Vanity URL: https://aka.ms/msal-net-on-behalf-of