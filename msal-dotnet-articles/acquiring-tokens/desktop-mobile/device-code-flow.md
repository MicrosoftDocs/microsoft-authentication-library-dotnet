---
title: Using Device Code Flow in MSAL.NET
description: "Interactive authentication with Azure AD requires a web browser. However, in the case of devices and operating systems that do not provide a Web browser, Device Code Flow lets the user use another device (for instance another computer or a mobile phone) to sign-in interactively."
---

# Using Device Code Flow in MSAL.NET

>[!NOTE]
>Device code flow is not supported by Azure AD B2C.

## Why would you use Device Code Flow?

Interactive authentication with Azure AD requires a web browser (for details see [Usage of web browsers](/azure/active-directory/develop/msal-net-web-browsers)). However, in the case of devices and operating systems that do not provide a Web browser, Device Code Flow lets the user use another device (for instance another computer or a mobile phone) to sign-in interactively. By using the device code flow, the application obtains tokens through a two-step process especially designed for these devices/OS. Examples of such applications are applications running on IoT, or Command-Line tools (CLI). The idea is that:

1. Whenever user authentication is required, the app provides a code and asks the user to use another device (such as an internet-connected smartphone) to navigate to a URL (for instance, `https://microsoft.com/devicelogin`), where the user will be prompted to enter the code. That done, the web page will lead the user through a normal authentication experience, including consent prompts and multi-factor authentication if necessary.

2. Upon successful authentication, the command-line app will receive the required tokens through a back channel and will use it to perform the web API calls it needs.

## Constraints

- Device Code Flow is only available on public client applications
- The authority passed in the <xref:Microsoft.Identity.Client.PublicClientApplicationBuilder> needs to be:
  - Tenanted, of the form `https://login.microsoftonline.com/{tenant}/` where `tenant` is either the guid representing the tenant ID or a domain associated with the tenant.

### Device Code Flow with Microsoft personal accounts

Starting with MSAL.NET 4.5 release, the device code flow is possible with Microsoft Personal Accounts. This means the device code flow will work with:

- Any work and school accounts (`https://login.microsoftonline.com/organizations/`), and
- Microsoft personal accounts (`/common` or `/consumers` tenants)

## How to use it?

### Application registration

During the **[App registration](https://go.microsoft.com/fwlink/?linkid=2083908)** , in the **Authentication** section for your application:

- The Reply URI should be `https://login.microsoftonline.com/common/oauth2/nativeclient`
- You need to choose **Yes** to the question **Treat application as a public client** (in the **Default client type** paragraph)

  ![Azure AD client type](../../media/azure-ad-client-type.png)

### Code

![IPublicClientApplication interface](../../media/ipublicclientapplication-interface.png)

`IPublicClientApplication`contains a method named `AcquireTokenWithDeviceCode`

```csharp
 AcquireTokenWithDeviceCode(IEnumerable<string> scopes, 
                            Func<DeviceCodeResult, Task> deviceCodeResultCallback)
```

This method takes as parameters:

- The `scopes` for which to request an access token
- A callback that will receive the `DeviceCodeResult`

  ![DeviceCoreResult class](../../media/devicecoderesult-class.png)

You can pass optional parameters, by calling:

- `.WithExtraQueryParameters(Dictionary{string, string})` to pass additional query parameters. This can be useful to target test environments, or for globalization (see below). You can pass `string.Empty`.
- `.WithAuthority(string, bool)` in order to override the default authority set at the application construction. Note that the overriding authority needs to be part of the known authorities added to the application construction.

## Code snippet

The following sample code presents the most current case, with explanations of the kind of exceptions you can get, and their mitigation.

```csharp
private const string ClientId = "<client_guid>";
private const string Authority = "https://login.microsoftonline.com/contoso.com";
private readonly string[] Scopes = new string[] { "user.read" };

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
        return await pca.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
            .ExecuteAsync();
    }
    catch (MsalUiRequiredException ex)
    {
        // No token found in the cache or Azure AD insists that a form interactive auth is required (e.g. the tenant admin turned on MFA)
        // If you want to provide a more complex user experience, check out ex.Classification 

        return await AcquireByDeviceCodeAsync(pca);
    }         
}

private async Task<AuthenticationResult> AcquireByDeviceCodeAsync(IPublicClientApplication pca)
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
    // TODO: handle or throw all these exceptions
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
        // See /dotnet/standard/threading/cancellation-in-managed-threads 
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

## Sample illustrating acquiring tokens through the Device Code Flow with MSAL.NET

Sample | Platform | Description
------ | -------- | -----------
[active-directory-dotnetcore-devicecodeflow-v2](https://github.com/Azure-Samples/active-directory-dotnetcore-devicecodeflow-v2) | Console (.NET Core) | .NET Core 2.1 console application letting a user acquire, with the Azure AD v2.0 endpoint, a token for the Microsoft Graph by singing in through another device having a Web browser ![Device code flow topology](../../media/device-code-flow-topology.png)

## Additional information

In case you want to learn more about Device code flow:

- [OAuth standard - device flow](https://tools.ietf.org/html/draft-ietf-oauth-device-flow-07#section-3.4)
