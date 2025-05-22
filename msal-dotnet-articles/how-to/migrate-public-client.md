---
title: Migrate public client applications to MSAL.NET
description: Learn how to migrate a public client application from Azure Active Directory Authentication Library for .NET to Microsoft Authentication Library for .NET.
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 05/20/2024
ms.service: msal
ms.subservice: msal-dotnet
ms.workload: identity
ms.reviewer:
ms.topic: how-to
ms.custom: devx-track-csharp, aaddev, has-adal-ref, devx-track-dotnet
#Customer intent: As an application developer, I want to migrate my public client app from ADAL.NET to MSAL.NET.

---

# Migrate public client applications from ADAL.NET to MSAL.NET

[!INCLUDE [ADAL migration note](../includes/adal-migration-note.md)]

This article describes how to migrate a public client application from Azure Active Directory Authentication Library for .NET (ADAL.NET) to Microsoft Authentication Library for .NET (MSAL.NET). Public client applications are desktop and mobile applications, including native (Win32) and WPF projects that call another service or API on behalf of a Microsoft Entra ID user. For more information about public client applications, see [Authentication flows and application scenarios](/entra/identity-platform/authentication-flows-app-scenarios).

## Migration steps

1. Find the code by using ADAL.NET in your app.

   The code that uses ADAL in a public client application instantiates `AuthenticationContext` and calls an override of `AcquireTokenAsync` with the following parameters:

   - A `resourceId` string. This variable is the app ID URI of the web API that you want to call.
   - A `clientId` which is the identifier for your application, also known as App ID.

2. After you've identified that you have applications that are using ADAL.NET, install the MSAL.NET NuGet package ([`Microsoft.Identity.Client`](https://www.nuget.org/packages/Microsoft.Identity.Client)) and update your project library references. For more information, see [An introduction to NuGet](/nuget/what-is-nuget).

3. Update the code according to the public client application scenario. Some steps are common and apply across all the public client scenarios. Other steps are unique to each scenario.

   The public client scenarios are:

   - [Web Account Manager](../acquiring-tokens/desktop-mobile/wam.md), the preferred authentication approach for Windows applications, using the authentication broker component.
   - [Interactive authentication](../acquiring-tokens/desktop-mobile/acquiring-tokens-interactively.md), where the user is shown a web-based interface to complete the sign-in process.
   - [Integrated Windows Authentication (IWA)](../acquiring-tokens/desktop-mobile/integrated-windows-authentication.md), where a user signs in using the same identity they signed into a Windows domain (for domain-joined or Microsoft Entra ID joined machines).
   - [Username/password](../acquiring-tokens/desktop-mobile/username-password-authentication.md), where the sign-in occurs by providing a username/password credential. **Microsoft does not recommend the username and password flow** because the application will be asking a user for their password directly, which is an insecure pattern.
   - [Device code flow](../acquiring-tokens/desktop-mobile/device-code-flow.md), where a device with limited UX capabilities shows the consumer a device code to complete the authentication flow on an alternative device.

## [Interactive](#tab/interactive)

Interactive scenarios are where your public client application shows a login user interface hosted in a browser, and the user is required to interactively sign-in.

#### Find out if your code uses interactive scenarios

The ADAL code for your app in a public client application that uses interactive authentication instantiates `AuthenticationContext` and includes a call to `AcquireTokenAsync`, with the following parameters.

- A `clientId` which is a GUID representing your application registration.
- A `resourceUrl` which indicates the resource you are asking the token for.
- A URI that is the reply URL.
- A `PlatformParameters` object.

#### Update the code for interactive scenarios

[!INCLUDE [Common steps](../includes/msal-net-adoption-steps-public-clients.md)]

In this case, we replace the call to `AuthenticationContext.AcquireTokenAsync` with a call to `IPublicClientApplication.AcquireTokenInteractive`.

Here's a comparison of ADAL.NET and MSAL.NET code for interactive scenarios:

:::row:::
:::column span="":::
    ADAL
:::column-end:::
:::column span="":::
    MSAL
:::column-end:::
:::row-end:::
:::row:::
:::column span="":::

```csharp
var ac = new AuthenticationContext("https://login.microsoftonline.com/<tenantId>");
AuthenticationResult result;
result = await ac.AcquireTokenAsync("<clientId>",
                                    "https://resourceUrl",
                                    new Uri("https://ClientReplyUrl"),
                                    new PlatformParameters(PromptBehavior.Auto));
```

:::column-end:::
:::column span="":::

```csharp
var scopes = new[] { "User.Read" };

BrokerOptions options = new BrokerOptions(BrokerOptions.OperatingSystems.Windows);
options.Title = "My Awesome Application";

IPublicClientApplication app =
    PublicClientApplicationBuilder.Create("YOUR_CLIENT_ID")
    .WithDefaultRedirectUri()
    .WithParentActivityOrWindow(GetConsoleOrTerminalWindow)
    .WithBroker(options)
    .Build();

AuthenticationResult result = null;

// Try to use the previously signed-in account from the cache
IEnumerable<IAccount> accounts = await app.GetAccountsAsync();
IAccount existingAccount = accounts.FirstOrDefault();

try
{    
    if (existingAccount != null)
    {
        result = await app.AcquireTokenSilent(scopes, existingAccount).ExecuteAsync();
    }
    // Next, try to sign in silently with the account that the user is signed into Windows
    else
    {    
        result = await app.AcquireTokenSilent(scopes, PublicClientApplication.OperatingSystemAccount)
                            .ExecuteAsync();
    }
}
// Can't get a token silently, go interactive
catch (MsalUiRequiredException ex)
{
    result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
}
```

   :::column-end:::
:::row-end:::

The MSAL code shown above uses WAM (Web Account Manager) which is the recommended approach for authenticating users on Windows. If you wish to use interactive authentication without WAM, see [Interactive Authentication](../acquiring-tokens/desktop-mobile/acquiring-tokens-interactively.md).

Refer to the [Using MSAL.NET with Web Account Manager (WAM)](../acquiring-tokens/desktop-mobile/wam.md) document for additional requirements to configure your application to use WAM.

>[!NOTE]
>WAM is only available on Windows. If you are building a cross-platform application, you need to make sure that you have a fallback to [interactive authentication without WAM](../acquiring-tokens/desktop-mobile/acquiring-tokens-interactively.md).

## [Integrated Windows Authentication (IWA)](#tab/iwa)

Integrated Windows Authentication enables your public client application to sign a user in using the same identity they used to sign into a Windows domain (for domain-joined or Microsoft Entra joined machines).

#### Find out if your code uses Integrated Windows Authentication

The ADAL code for your app uses integrated Windows authentication scenarios if it contains a call to `AcquireTokenAsync` available as an extension method of the `AuthenticationContextIntegratedAuthExtensions` class, with the following parameters:

- A `resource` which represents the resource you are asking the token for
- A `clientId` which is a GUID representing your application registration
- A `UserCredential` object that represents the user you are trying to request the token for.

#### Update the code for Integrated Windows Authentication scenarios

[!INCLUDE [Common steps](../includes/msal-net-adoption-steps-public-clients.md)]

In this case, we replace the call to `AuthenticationContext.AcquireTokenAsync` with a call to `IPublicClientApplication.AcquireTokenByIntegratedWindowsAuth`.

Here's a comparison of ADAL.NET and MSAL.NET code for integrated Windows Authentication scenarios:

:::row:::
:::column span="":::
    ADAL
:::column-end:::
:::column span="":::
    MSAL
:::column-end:::
:::row-end:::
:::row:::
:::column span="":::

```csharp
var ac = new AuthenticationContext("https://login.microsoftonline.com/<tenantId>");
AuthenticationResult result;
result = await context.AcquireTokenAsync(resource, clientId,
                                         new UserCredential("john@contoso.com"));
```

:::column-end:::
:::column span="":::

```csharp
 string authority = "https://login.microsoftonline.com/contoso.com";
 string[] scopes = new string[] { "user.read" };
 IPublicClientApplication app = PublicClientApplicationBuilder
      .Create(clientId)
      .WithAuthority(authority)
      .Build();

 var accounts = await app.GetAccountsAsync();

 AuthenticationResult result = null;
 if (accounts.Any())
 {
  result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
      .ExecuteAsync();
 }
 else
 {
  try
  {
   result = await app.AcquireTokenByIntegratedWindowsAuth(scopes)
      .ExecuteAsync(CancellationToken.None);
  }
  catch (MsalUiRequiredException ex)
  {
   // MsalUiRequiredException: AADSTS65001: The user or administrator has not consented to use the application
   // with ID '{appId}' named '{appName}'.Send an interactive authorization request for this user and resource.

   // you need to get user consent first. This can be done, if you are not using .NET Core (which does not have any Web UI)
   // by doing (once only) an AcquireToken interactive.

   // If you are using .NET core or don't want to do an AcquireTokenInteractive, you might want to suggest the user to navigate
   // to a URL to consent: https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&scope=user.read

   // AADSTS50079: The user is required to use multi-factor authentication.
   // There is no mitigation - if MFA is configured for your tenant and Azure AD decides to enforce it,
   // you need to fallback to an interactive flows such as AcquireTokenInteractive or AcquireTokenByDeviceCode
   }
   catch (MsalServiceException ex)
   {
    // Kind of errors you could have (in ex.Message)

    // MsalServiceException: AADSTS90010: The grant type is not supported over the /common or /consumers endpoints. Please use the /organizations or tenant-specific endpoint.
    // you used common.
    // Mitigation: as explained in the message from Azure AD, the authority needs to be tenanted or otherwise organizations

    // MsalServiceException: AADSTS70002: The request body must contain the following parameter: 'client_secret or client_assertion'.
    // Explanation: this can happen if your application was not registered as a public client application in Azure AD
    // Mitigation: in the Azure portal, edit the manifest for your application and set the `allowPublicClient` to `true`
   }
   catch (MsalClientException ex)
   {
      // Error Code: unknown_user Message: Could not identify logged in user
      // Explanation: the library was unable to query the current Windows logged-in user or this user is not AD or Azure AD
      // joined (work-place joined users are not supported).

      // Mitigation: Implement your own logic to fetch the username (e.g. john@contoso.com) and use the
      // AcquireTokenByIntegratedWindowsAuth form that takes in the username

      // Error Code: integrated_windows_auth_not_supported_managed_user
      // Explanation: This method relies on a protocol exposed by Active Directory (AD). If a user was created in Azure
      // Active Directory without AD backing ("managed" user), this method will fail. Users created in AD and backed by
      // Azure AD ("federated" users) can benefit from this non-interactive method of authentication.
      // Mitigation: Use interactive authentication
   }
 }

 Console.WriteLine(result.Account.Username);
}
```

   :::column-end:::
:::row-end:::

## [Username and password](#tab/up)

Username and password authentication is where the sign-in occurs by providing a username and password credential directly to the application.

>[!WARNING]
>**Microsoft does not recommend this flow because it presents security risks that are not present in other flows**. If you're using the username and password flow in production, we recommend switching to other, more secure alternatives outlined in this article.

#### Find out if your code uses username and password authentication

The ADAL code for your app uses username password authentication scenarios if it contains a call to `AcquireTokenAsync` available as an extension method of the `AuthenticationContextIntegratedAuthExtensions` class, with the following parameters:

- A `resource` which represents the resource you are asking the token for.
- A `clientId` which is a GUID representing your application registration.
- A `UserPasswordCredential` object that contains the username and password for the user you are trying to request the token for.

#### Update the code for username password auth scenarios

In this case, we replace the call to `AuthenticationContext.AcquireTokenAsync` with a call to `IPublicClientApplication.AcquireTokenByUsernamePassword`.

Here's a comparison of ADAL.NET and MSAL.NET code for username password scenarios:

[!INCLUDE [Common steps](../includes/msal-net-adoption-steps-public-clients.md)]

:::row:::
:::column span="":::
    ADAL
:::column-end:::
:::column span="":::
    MSAL
:::column-end:::
:::row-end:::
:::row:::
:::column span="":::

```csharp
var ac = new AuthenticationContext("https://login.microsoftonline.com/<tenantId>");
AuthenticationResult result;
result = await context.AcquireTokenAsync(
   resource, clientId, 
   new UserPasswordCredential("john@contoso.com", johnsPassword));

```

:::column-end:::
:::column span="":::

```csharp
 string authority = "https://login.microsoftonline.com/contoso.com";
 string[] scopes = new string[] { "user.read" };
 IPublicClientApplication app;
 app = PublicClientApplicationBuilder.Create(clientId)
       .WithAuthority(authority)
       .Build();
 var accounts = await app.GetAccountsAsync();

 AuthenticationResult result = null;
 if (accounts.Any())
 {
  result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
 }
 else
 {
  try
  {
   var securePassword = new SecureString();
   foreach (char c in "dummy")        // you should fetch the password
    securePassword.AppendChar(c);  // keystroke by keystroke

   result = await app.AcquireTokenByUsernamePassword(scopes,
                                                    "joe@contoso.com",
                                                     securePassword)
                      .ExecuteAsync();
  }
  catch(MsalException)
  {
   // See details below
  }
 }
 Console.WriteLine(result.Account.Username);
```

   :::column-end:::
:::row-end:::

## [Device code](#tab/devicecode)

Device code flow authentication is where a device with limited UX shows you a device code to complete the authentication flow on an alternative device.

#### Find out if your code uses device code flow authentication

The ADAL code for your app uses device code flow scenarios if it contains a call to `AuthenticationContext.AcquireTokenByDeviceCodeAsync` with the following parameters:

- A `DeviceCodeResult` object instance, which is instantiated with the `resourceID` of the resource you are asking for a token for, and a `clientId` which is the GUID that represents your application.

#### Update the code for device code flow scenarios

 [!INCLUDE [Common steps](../includes/msal-net-adoption-steps-public-clients.md)]

In this case, we replace the call to `AuthenticationContext.AcquireTokenAsync` with a call to `IPublicClientApplication.AcquireTokenWithDeviceCode`.

Here's a comparison of ADAL.NET and MSAL.NET code for device code flow scenarios:

:::row:::
:::column span="":::
    ADAL
:::column-end:::
:::column span="":::
    MSAL
:::column-end:::
:::row-end:::
:::row:::
:::column span="":::

```csharp
static async Task<AuthenticationResult> GetTokenViaCode(AuthenticationContext ctx)
{
 AuthenticationResult result = null;
 try
 {
  result = await ac.AcquireTokenSilentAsync(resource, clientId);
 }
 catch (AdalException adalException)
 {
  if (adalException.ErrorCode == AdalError.FailedToAcquireTokenSilently
   || adalException.ErrorCode == AdalError.InteractionRequired)
  {
  try
  {
   DeviceCodeResult codeResult = await ctx.AcquireDeviceCodeAsync(resource, clientId);
   Console.WriteLine("You need to sign in.");
   Console.WriteLine("Message: " + codeResult.Message + "\n");
   result = await ctx.AcquireTokenByDeviceCodeAsync(codeResult);
  }
  catch (Exception exc)
  {
   Console.WriteLine("Something went wrong.");
   Console.WriteLine("Message: " + exc.Message + "\n");
  }
 }
 return result;
}

```

:::column-end:::
:::column span="":::

```csharp
private const string ClientId = "<client_guid>";
private const string Authority = "https://login.microsoftonline.com/contoso.com";
private readonly string[] scopes = new string[] { "user.read" };

static async Task<AuthenticationResult> GetATokenForGraph()
{
    IPublicClientApplication pca = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority(Authority)
            .WithDefaultRedirectUri()
            .Build();

    var accounts = await pca.GetAccountsAsync();

    // All AcquireToken* methods store the tokens in the cache, so check the cache first
    try
    {
        return await pca.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
            .ExecuteAsync();
    }
    catch (MsalUiRequiredException ex)
    {
        // No token found in the cache or Azure AD insists that a form interactive auth is required (e.g. the tenant admin turned on MFA)
        // If you want to provide a more complex user experience, check out ex.Classification

        return await AcquireByDeviceCodeAsync(pca);
    }
}

private static async Task<AuthenticationResult> AcquireByDeviceCodeAsync(IPublicClientApplication pca)
{
    try
    {
        var result = await pca.AcquireTokenWithDeviceCode(scopes,
            deviceCodeResult =>
            {
                    // This will print the message on the console which tells the user where to go sign-in using
                    // a separate browser and the code to enter once they sign in.
                    // The AcquireTokenWithDeviceCode() method will poll the server after firing this
                    // device code callback to look for the successful login of the user via that browser.
                    // This background polling (whose interval and timeout data is also provided as fields in the
                    // deviceCodeCallback class) will occur until:
                    // * The user has successfully logged in via browser and entered the proper code
                    // * The timeout specified by the server for the lifetime of this code (typically ~15 minutes) has been reached
                    // * The developing application calls the Cancel() method on a CancellationToken sent into the method.
                    //   If this occurs, an OperationCanceledException will be thrown (see catch below for more details).
                    Console.WriteLine(deviceCodeResult.Message);
                return Task.FromResult(0);
            }).ExecuteAsync();

        Console.WriteLine(result.Account.Username);
        return result;
    }

    // TODO: handle or throw all these exceptions depending on your app
    catch (MsalServiceException ex)
    {
        // Kind of errors you could have (in ex.Message)

        // AADSTS50059: No tenant-identifying information found in either the request or implied by any provided credentials.
        // Mitigation: as explained in the message from Azure AD, the authoriy needs to be tenanted. you have probably created
        // your public client application with the following authorities:
        // https://login.microsoftonline.com/common or https://login.microsoftonline.com/organizations

        // AADSTS90133: Device Code flow is not supported under /common or /consumers endpoint.
        // Mitigation: as explained in the message from Azure AD, the authority needs to be tenanted

        // AADSTS90002: Tenant <tenantId or domain you used in the authority> not found. This may happen if there are
        // no active subscriptions for the tenant. Check with your subscription administrator.
        // Mitigation: if you have an active subscription for the tenant this might be that you have a typo in the
        // tenantId (GUID) or tenant domain name.
    }
    catch (OperationCanceledException ex)
    {
        // If you use a CancellationToken, and call the Cancel() method on it, then this *may* be triggered
        // to indicate that the operation was cancelled.
        // See https://learn.microsoft.com/dotnet/standard/threading/cancellation-in-managed-threads
        // for more detailed information on how C# supports cancellation in managed threads.
    }
    catch (MsalClientException ex)
    {
        // Possible cause - verification code expired before contacting the server
        // This exception will occur if the user does not manage to sign-in before a time out (15 mins) and the
        // call to `AcquireTokenWithDeviceCode` is not cancelled in between
    }
}
```

   :::column-end:::
:::row-end:::

---

### MSAL benefits

Learn about MSAL library benefits in the [Migrate applications to the Microsoft Authentication Library (MSAL)](/entra/identity-platform/msal-migration) article.

[!INCLUDE [importance-of-logging](../includes/importance-of-logging.md)]

### Troubleshooting

The following troubleshooting information makes two assumptions:

- Your ADAL.NET code was working.
- You migrated to MSAL by keeping the same client ID.

If you get an exception with the following message:

> `AADSTS90002: Tenant 'aaaabbbb-0000-cccc-1111-dddd2222eeee' not found. This may happen if there are no active`
> `subscriptions for the tenant. Check to make sure you have the correct tenant ID. Check with your subscription`
> `administrator.`

You can troubleshoot the exception by using these steps:

1. Confirm that you're using the latest version of MSAL.NET.
1. Confirm that the authority host that you set when building the confidential client application and the authority host that you used with ADAL are similar. In particular, is it the same [cloud](/entra/identity-platform/msal-national-cloud) (Azure Government, Microsoft Azure operated by 21Vianet, or Azure Germany)?

## Next steps

- Learn more about the [differences between ADAL.NET and MSAL.NET apps](differences-adal-msal-net.md).
- Learn more about [token cache serialization in MSAL.NET](token-cache-serialization.md).
