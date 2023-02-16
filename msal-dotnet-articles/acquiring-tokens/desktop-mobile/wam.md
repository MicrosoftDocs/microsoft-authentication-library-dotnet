MSAL is able to call Web Account Manager (WAM), a Windows 10+ component that ships with the OS. This component acts as an authentication broker allowing the users of your app benefit from integration with accounts known to Windows, such as the account you signed into your Windows session.

## New WAM Preview in MSAL 4.44+
The new MSAL WAM Preview is an abstraction layer based on MSAL C++ which fixes a number of issues with the existing WAM implementation and provides other benefits. **New applications should use this implementation** (also see [WAM limitations](#wam-limitations)).

- New implementation is more stable, easier to add new features, less chance of regressions.
- Works in apps that are run-as-admin.
- Adds support for Proof-of-Possession tokens.
- Fixes assembly size issues.

### To enable WAM preview 

- add a reference to [Microsoft.Identity.Client.Broker](https://www.nuget.org/packages/Microsoft.Identity.Client.Broker) (and you can remove any reference to Microsoft.Identity.Client.Desktop)
- instead of `.WithBroker()`, call `.WithBrokerPreview()`.

> **Note:** The old WAM experience is documented at [Acquire a token using WAM
](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-desktop-acquire-token-wam) and showcases details about redirect URI, fallback experience on older Windows, Mac and Linux, etc. which remain valid.

```csharp
var pca = PublicClientApplicationBuilder
              .Create("1234...")
              .WithAuthority("https://login.microsoftonline.com/common")
              .WithBrokerPreview(true)   // this method exists in Microsoft.Identity.Client.Broker package
              .Build();
```
## Parent Window Handles

It is now mandatory to tell MSAL the window the interactive experience should be parented to, using `WithParentActivityOrWindow` APIs. Trying to infer a window is not feasible and in the past, this has led to bad user experience where the auth window is hidden behind the application. 

```csharp

// In a WinForms app, in a Form
IntPtr windowHandle = this.Handle;

// In a WPF app 
IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
```

For console applications it is a bit more involved, because of the terminal window and its tabs. 

```csharp
enum GetAncestorFlags
{   
    GetParent = 1,
    GetRoot = 2,
    /// <summary>
    /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
    /// </summary>
    GetRootOwner = 3
}

/// <summary>
/// Retrieves the handle to the ancestor of the specified window.
/// </summary>
/// <param name="hwnd">A handle to the window whose ancestor is to be retrieved.
/// If this parameter is the desktop window, the function returns NULL. </param>
/// <param name="flags">The ancestor to be retrieved.</param>
/// <returns>The return value is the handle to the ancestor window.</returns>
[DllImport("user32.dll", ExactSpelling = true)]
static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

[DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();

public IntPtr GetConsoleOrTerminalWindow()
{
   IntPtr consoleHandle = GetConsoleWindow();
   IntPtr handle = GetAncestor(consoleHandle, GetAncestorFlags.GetRootOwner );
  
   return handle;
}
```

> The logic below will eventually be added as a helper in MSAL, tracking work item [3590](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/3590). For now, copy this into your app.


## Proof of Possession Access Tokens

MSAL already supports [PoP tokens in confidential client flows](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Proof-Of-Possession-%28PoP%29-tokens) starting MSAL 4.8+, With the new MSAL WAM Broker you can acquire [PoP tokens for public client flows](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Proof-Of-Possession-(PoP)-tokens) as well. 

Bearer tokens are the norm in modern identity flows, however they are vulnerable to being stolen and used to access a protected resource. 

Proof of Possession (PoP) tokens mitigate this threat via 2 mechanisms: 

- they are bound to the user / machine that wants to access a protected resource, via a public / private key pair
- they are bound to the protected resource itself, i.e. a token that is used to access `GET https://contoso.com/transactions` cannot be used to access `GET https://contoso.com/tranfer/100`

In order to utilize the new broker to perform POP, see the code snippet [here](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Proof-Of-Possession-(PoP)-tokens#proof-of-possession-for-public-clients).

For more details, see [RFC 7800](https://tools.ietf.org/html/rfc7800)

## Redirect URI

WAM redirect URIs do not need to be configured in MSAL, but they must be configured in the app registration.

```
ms-appx-web://microsoft.aad.brokerplugin/{client_id}
```

## Username / Password flow 

This flow is not recommended except in test scenarios or in scenarios where service principal access to a resource gives it too much access and you can only scope it down with user flows. When using WAM, `AcquireTokenByUsernamePassword` will let WAM handle the protocol and fetch tokens. 

## WAM limitations

- B2C and ADFS authorities aren't supported. MSAL will fall back to a browser.
- Available on Windows 10+ and Windows Server 2019+. On Mac, Linux, and earlier versions of Windows, MSAL will fall back to a browser.
- WAM Preview is not available on UWP; instead use the old WAM implementation.

## Troubleshooting

### "Unable to load DLL 'msalruntime' or one of its dependencies: The specified module could not be found." error message

This message indicates that either the [Microsoft.Identity.Client.NativeInterop](https://www.nuget.org/packages/Microsoft.Identity.Client.NativeInterop) package was not properly installed or the WAM runtimes DLL was not restored in the appropriate folders. To resolve this issue: 

1. Ensure that [Microsoft.Identity.Client.NativeInterop](https://www.nuget.org/packages/Microsoft.Identity.Client.NativeInterop) package has been restored properly, and
1. the runtimes folders are also restored and placed under the package path

The DLL search order is,

- same directory as the app (executing assembly directory)
- other directories like system and windows
- `runtimes` folder under the NuGet [global-packages](https://learn.microsoft.com/en-us/nuget/consume-packages/managing-the-global-packages-and-cache-folders) folder where [Microsoft.Identity.Client.NativeInterop](https://www.nuget.org/packages/Microsoft.Identity.Client.NativeInterop) is installed. 

![image](https://user-images.githubusercontent.com/90415114/193084876-f67638a2-7a10-4b6e-8943-43c851be8687.png)

### "MsalClientException (ErrCode 5376): At least one scope needs to be requested for this authentication flow." error message

This message indicates that you need to request at least one application scope (e.g. user.read) along with other OIDC scopes (profile, email or offline_access). 

```
var authResult = await pca.AcquireTokenInteractive(new[] { "user.read" })
                                      .ExecuteAsync();
``` 

### Account Picker does not show up

Sometimes a Windows update messes up the Account Picker component - which shows the list of acccounts in Windows and the option to add new accounts. The symptom is that the picker does not come up for a small number of users. 

A possible workaround is to re-register this component. Run this script from an Admin powershell console:

```powerhsell
if (-not (Get-AppxPackage Microsoft.AccountsControl)) { Add-AppxPackage -Register "$env:windir\SystemApps\Microsoft.AccountsControl_cw5n1h2txyewy\AppxManifest.xml" -DisableDevelopmentMode -ForceApplicationShutdown } Get-AppxPackage Microsoft.AccountsControl
```

### Connection issues

The application user sees an error message similar to "Please check your connection and try again". If this issue occurs regularly, see the [troubleshooting guide for Office](https://learn.microsoft.com/en-us/microsoft-365/troubleshoot/authentication/connection-issue-when-sign-in-office-2016), which also uses WAM.