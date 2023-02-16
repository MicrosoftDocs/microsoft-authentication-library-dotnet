> This page is for MSAL 2.x
> 
> If you are interested in MSAL 3.x, please see [Device code flow in MSAL](Device-Code-Flow)

## Why would you use Device Code Flow?

Interactive authentication with Azure AD requires a web browser (for details see [Usage of web browsers](MSAL.NET-uses-web-browser)). However, in the case of devices and operating systems that do not provide a Web browser, Device code flow lets the user use another device (for instance another computer or a mobile phone) to sign-in interactively. By using the device code flow, the application obtains tokens through a two-step process especially designed for these devices/OS. Examples of such applications are applications running on iOT, or Command-Line tools (CLI). The idea is that:

1. Whenever a user authentication is required, the app provides a code and asks the user to use another device (such as an internet-connected smartphone) to navigate to a URL (for instance http://microsoft.com/devicelogin), where the user will be prompted to enter the code. That done, the web page will lead the user through a normal authentication experience, including consent prompts and multi-factor authentication if necessary.

2. Upon successful authentication, the command-line app will receive the required tokens through a back channel and will use it to perform the web API calls it needs.

## Constraints

- MSAL.NET 2.2.0 and above
- Device Code Flow is only available on public client applications
- The authority passed to the constructor of `PublicClientApplication` needs to be:
  - tenanted (of the form `https://login.microsoftonline.com/{tenant}/` where `tenant` is either the guid representing the tenant ID or a domain associated with the tenant.
  - or any work and school accounts (`https://login.microsoftonline.com/organizations/`)

  > Microsoft personal accounts are not yet supported by the Azure AD v2.0 endpoint (you cannot use /common or /consumers tenants)

## How to use it?

`PublicClientApplication`contains two overrides of `AcquireTokenWithDeviceCodeAsync`. The second is like the first but enables cancellation of the call.

![image](https://user-images.githubusercontent.com/13203188/46328474-4364c080-c5bc-11e8-8865-582d1933eb2d.png)

These methods take as parameters:
- the `scopes` to request an access token for
- a string enabling passing extra query parameters. This can be useful to target test environments, or for globalization (see below). you can pass string.Empty.
- a callback that will receive the `DeviceCodeResult`
- an optional `CancellationToken` in case you want the enable the application to cancel the call.

## Code snippet

The following sample code presents the most current case, with explanations of the kind of exceptions you can get, and their mitigations

```CSharp
static async Task<AuthenticationResult> GetATokenForGraph()
{
 string authority = "https://login.microsoftonline.com/contoso.com";
 string[] scopes = new string[] { "user.read" };
 PublicClientApplication app = new PublicClientApplication(clientId, authority);

 AuthenticationResult result = null;
 var accounts = await app.GetAccountsAsync();

 // All AcquireToken* methods store the tokens in the cache, so check the cache first
 try
 {
  result = await app.AcquireTokenSilentAsync(scopes, accounts.FirstOrDefault());
  return result;
 }
 catch (MsalUiRequiredException ex)
 {
  // A MsalUiRequiredException happened on AcquireTokenSilentAsync. 
  // This indicates you need to call AcquireTokenAsync to acquire a token
  System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");
 }

 try
 {
  result = await app.AcquireTokenWithDeviceCodeAsync(scopes,
      string.Empty, deviceCodeResult =>
  {
       // This will print the message on the console which tells the user where to go sign-in using 
       // a separate browser and the code to enter once they sign in.
       // The AcquireTokenWithDeviceCodeAsync() method will poll the server after firing this
       // device code callback to look for the successful login of the user via that browser.
       // This background polling (whose interval and timeout data is also provided as fields in the 
       // deviceCodeCallback class) will occur until:
       // * The user has successfully logged in via browser and entered the proper code
       // * The timeout specified by the server for the lifetime of this code (typically ~15 minutes) has been reached
       // * The developing application calls the Cancel() method on a CancellationToken sent into the method.
       //   If this occurs, an OperationCanceledException will be thrown (see catch below for more details).
       Console.WriteLine(deviceCodeResult.Message);
       return Task.FromResult(0);
  }, CancellationToken.None);

  Console.WriteLine(result.Account.Username);
  return result;
 }
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
  // If you use a CancellationToken, and call the Cancel() method on it, then this may be triggered
  // to indicate that the operation was cancelled. 
  // See https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads 
  // for more detailed information on how C# supports cancellation in managed threads.
 }
 catch (MsalClientException ex)
 {
  // Verification code expired before contacting the server
  // This exception will occur if the user does not manage to sign-in before a time out (15 mins) and the
  // call to `AcquireTokenWithDeviceCodeAsync` is not cancelled in between
 }
}
```

## Sample illustrating acquiring tokens through the Device Code Flow with MSAL.NET
Sample | Platform | Description 
------ | -------- | -----------
[active-directory-dotnetcore-devicecodeflow-v2](https://github.com/Azure-Samples/active-directory-dotnetcore-devicecodeflow-v2) | Console (.NET Core) | .NET Core 2.1 console application letting a user acquire, with the Azure AD v2.0 endpoint, a token for the Microsoft Graph by singing in through another device having a Web browser ![](https://github.com/Azure-Samples/active-directory-dotnetcore-devicecodeflow-v2/blob/master/ReadmeFiles/Topology.png)

## Additional information

In case you want to learn more about Device code flow:
- [OAuth standard - device flow](https://tools.ietf.org/html/draft-ietf-oauth-device-flow-07#section-3.4)
- How this was done with the V1 endpoint: [Device code flow in ADAL.NET](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Device-profile-for-devices-without-web-browsers)

<!-- 
- [2.0 Protocols - OAuth 2.0 device code flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-device-code-flow) to come
-->