---
title: Using MSAL.Net with broker on Linux
description: Learn how to integrate Microsoft Entra ID authentication in native Linux apps using MSAL.NET and the authentication broker.
author: ploegert
ms.author: jploegert
ms.service: msal
ms.topic: how-to
ms.date: 05/08/2025
---

# Enable SSO in native Linux apps using MSAL.NET

Microsoft Authentication Library (MSAL) is a Software Development Kit (SDK) that enables apps to call the Microsoft Single Sign-on to Linux broker, a Linux component that is shipped independent of the Linux Distribution, however it gets installed using a package manager using `sudo apt install microsoft-identity-broker` or `sudo dnf install microsoft-identity-broker`.

This component acts as an authentication broker, allowing the users of your app to benefit from integration with accounts known to Linux - such as the account you signed into your Linux sessions for apps that consume from the broker.

The broker is also bundled as a dependency of applications developed by Microsoft (such as [Company Portal](/mem/intune-service/user-help/enroll-device-linux))). An example of installation of the broker being installed is when a Linux computer is enrolled into a company's device fleet via an endpoint management solution like [Microsoft Intune](/mem/intune/fundamentals/what-is-intune).

> [!NOTE]
> Microsoft single sign-on (SSO) for Linux authentication broker support is introduced with `Microsoft.Identity.Client` version v4.69.1.

## What is a broker

An authentication broker is an application that runs on a user’s machine that manages the authentication handshakes and token maintenance for connected accounts. The Linux operating system uses the Microsoft single sign-on for Linux as its authentication broker. It has many benefits for developers and customers alike, including:

- **Enables Single Sign-On**: enables apps to simplify how users authenticate with Microsoft Entra ID and protects Microsoft Entra ID refresh tokens from exfiltration and misuse
- **Enhanced security.** Many security enhancements are delivered with the broker, without needing to update the application logic.
- **Feature support.** With the help of the broker developers can access rich OS and service capabilities.
- **System integration.** Applications that use the broker plug-and-play with the built-in account picker, allowing the user to quickly pick an existing account instead of reentering the same credentials over and over.
- **Token Protection.** Microsoft single sign-on for Linux ensures that the refresh tokens are device bound and [enables apps](../../advanced/proof-of-possession-tokens.md) to acquire device bound access tokens. See [Token Protection](/azure/active-directory/conditional-access/concept-token-protection).

## User sign-in experience

This video demonstrates the sign-in experience on brokered flows on Linux

![Demo of the Linux Login component component](../../media/linux/linux-entra-login.gif)

## How to opt in to use broker?

### Update Application Definition

In the MSAL Python library, we've introduced the `enable_broker_on_linux` flag, which enables the broker on both WSL and standalone Linux.
- If your goal is to enable broker support solely on WSL for Azure CLI, you can consider modifying the Azure CLI app code to activate the `enable_broker_on_wsl` flag exclusively on WSL.
- If you are writing a cross-platform application, you'll also need to use `enable_broker_on_windows`, as outlined in the [Using MSAL Python with Web Account Manager](wam.md) article.
- You can set any combination of the following opt-in parameters to true:

| Opt-in flag              | If app runs on                | App has registered this as a Desktop platform redirect URI in Azure portal       |
| ------------------------ | --------------------------------- | -------------------------------------------------------------------------------- |
| enable_broker_on_windows | Windows 10+                       | ms-appx-web://Microsoft.AAD.BrokerPlugin/your_client_id                          |
| enable_broker_on_wsl     | WSL                               | ms-appx-web://Microsoft.AAD.BrokerPlugin/your_client_id                          |
| enable_broker_on_mac     | Mac with Company Portal installed | msauth.com.msauth.unsignedapp://auth                                             |
| enable_broker_on_linux   | Linux with Intune installed       | `https://login.microsoftonline.com/common/oauth2/nativeclient` (MUST be enabled) |

Your application needs to support broker-specific redirect URIs. For `Linux` specifically, the URL for the redirect URI must be:

```text
https://login.microsoftonline.com/common/oauth2/nativeclient
```

### .NET Installation

Identity integration dependent on having dotnet 8 installed on the Linux distribution, and recommend installing via the [installation script](/dotnet/core/install/linux-scripted-manual#scripted-install).

```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --version latest
```

### Package Dependencies 

Install the following dependencies on your Linux platform:

- `libsecret-tools` is required to interface with the Linux keychain
- `libx11-6` package, where the `libx11` library is used to get the console window handle on Linux.

### [Ubuntu](#tab/ubuntudep)

To install on debian/Ubuntu based Linux distribution:

```bash
sudo apt install libx11-6 libc++1 libc++abi1 libsecret-1-0 libwebkit2gtk-4.0-37 -y
```

### [Red Hat Enterprise Linux](#tab/rheldep)

To install on Red Hat/Fedora based Linux distribution:

```bash
sudo dnf install libx11-6 libc++1 libc++abi1 libsecret-1-0 libwebkit2gtk-4.0-37 -y
```

---

## Parent window handles

To use the broker, apps must provide the window handle to which the modal dialog be parented using `libx11` library. The window handle must be provided by the developer because it's infeasible for MSAL itself to infer the parent window. In the past, lack of handling parent window leads to bad user experiences where the authentication window was hidden behind the application window.

For console applications, here’s sample code to use `libx11`:

```csharp
using System;
using System.Runtime.InteropServices;

class X11Interop
{
    [DllImport("libX11")]
    public static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11")]
    public static extern IntPtr XDefaultRootWindow(IntPtr display);

    public static void Main()
    {
        IntPtr display = XOpenDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero)
        {
            Console.WriteLine("Unable to open X display.");
            return;
        }

        IntPtr rootWindow = XDefaultRootWindow(display);
        Console.WriteLine($"Root window handle: {rootWindow}");
    }
}
```

## Run a Sample App

To set up a test app, you can either create your own console app as shown below, or use and update the sample app provided in [microsoft-authentication-library-for-dotnet](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) under the path [/tests/devapps/WAM/NetWSLWam/Class1.cs](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/main/tests/devapps/WAM/NetWSLWam/Class1.cs)

To use a broker on the Linux platform, set the `BrokerOptions` to `OperatingSystems.Linux` as shown in the below code snippet:

A sample application is available in the [MSAL.NET GitHub repository](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/tree/main/tests/devapps/WAM/NetWSLWam).

```csharp
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;

class Program
{
    /// <summary>
    /// Get the handle of the console window for Linux
    /// </summary>
    [DllImport("libX11")]
    private static extern IntPtr XOpenDisplay(string display);

    [DllImport("libX11")]
    private static extern IntPtr XRootWindow(IntPtr display, int screen);

    [DllImport("libX11")]
    private static extern IntPtr XDefaultRootWindow(IntPtr display);

    public static string ClientID = "your client id";
    public static string[] Scopes = { "User.Read" };

    public static async Task InvokeBrokerAsync()
    {
        IntPtr _parentHandle = XRootWindow(XOpenDisplay(null), 0);
        if (_parentHandle == IntPtr.Zero)
        {
            Console.WriteLine("Unable to open X display.");
            return;
        }

        Func<IntPtr> consoleWindowHandleProvider = () => _parentHandle;

        // 1. Configuration - read below about redirect URI
        var pca = PublicClientApplicationBuilder.Create(ClientID)
                        .WithAuthority("https://login.microsoftonline.com/common")
                        .WithDefaultRedirectUri()
                        .WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Linux){
                            ListOperatingSystemAccounts = true,
                            MsaPassthrough = true,
                            Title = "MSAL WSL Test App"
                        })
                        .WithParentActivityOrWindow(consoleWindowHandleProvider)
                        .WithLogging((x, y, z) => Console.WriteLine($"{x} {y}"), LogLevel.Verbose, true)
                        .Build();

        // Add a token cache, see https://learn.microsoft.com/entra/msal/dotnet/how-to/token-cache-serialization?tabs=desktop

        // 2. GetAccounts
        var accounts = await pca.GetAccountsAsync().ConfigureAwait(false);
        var accountToLogin = accounts.FirstOrDefault();

        try
        {
            var authResult = await pca.AcquireTokenSilent(Scopes, accountToLogin)
                                    .ExecuteAsync().ConfigureAwait(false);
        }
        catch (MsalUiRequiredException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.ErrorCode);
        }

        try
        {
            var authResult = await pca.AcquireTokenInteractive(Scopes)
                                    .ExecuteAsync().ConfigureAwait(false);

            Console.WriteLine(authResult.Account);

            Console.WriteLine("Acquired Token Successfully!!!");

            Console.WriteLine("Account: " + authResult.Account.Username);
            Console.WriteLine("Token: " + authResult.AccessToken);
            Console.WriteLine("Expires On: " + authResult.ExpiresOn.ToString());
            Console.WriteLine("Scopes: " + string.Join(", ", authResult.Scopes));
        }
        catch (MsalUiRequiredException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.ErrorCode);

        }
        catch (MsalClientException ex)
        {
            int errorCode = Marshal.GetHRForException(ex) & ((1 << 16) - 1);
            Console.WriteLine("MsalClientException (ErrCode " + errorCode + "): " + ex.Message);
        }
        catch (MsalException ex)
        {
            Console.WriteLine($"MsalException Error signing-out user: {ex.Message}");
        }
        catch (Exception ex)
        {
            int errorCode = Marshal.GetHRForException(ex) & ((1 << 16) - 1);
            Console.WriteLine("Error Acquiring Token (ErrCode " + errorCode + "): " + ex);
        }
        Console.Read();
    }

    public static void Main(string[] args)
    {
        InvokeBrokerAsync().Wait();
    }
}

```

To run the sample app:

```bash
# Run From the root folder of microsoft-authentication-library-dotnet directory
dotnet run --project tests/devapps/WAM/NetWSLWam/test.csproj
```

## Username/password flow

This flow, also known as Resource Owner Password Credentials (ROPC), isn't recommended except in test scenarios or in scenarios where service principal access to a resource gives it too much access and you can only scope it down with user flows. When using the broker, [`AcquireTokenByUsernamePassword`](xref:Microsoft.Identity.Client.PublicClientApplication.AcquireTokenByUsernamePassword*) lets the broker manage the protocol and fetch tokens.

>[!WARNING]
>Microsoft doesn't recommend using the username and password flow as the application asks a user for their password directly, which is an insecure pattern. Additionally, the ROPC flow **doesn't support personal Microsoft accounts** and **Microsoft Entra accounts with enabled multi-factor authentication**. Check out [Microsoft identity platform and OAuth 2.0 Resource Owner Password Credentials](/azure/active-directory/develop/v2-oauth-ropc) for the full overview.

## Microsoft single sign-on for Linux limitations

- Azure B2C and Active Directory Federation Services (ADFS) authorities aren't supported. MSAL falls back to using a browser for user authentication.
- On unsupported configurations, MSAL falls back to a browser.

## Integration best practices

To make sure that your customers have a great experience with MSAL+Broker, we strongly advise you adhere to the following principles:

1. **Give the user context prior to authentication**. Draw a UI or window that informs the user that they need to authenticate, along with reasons for authentication. Explain the benefits of your application if it's a background service.
2. **Invoke authentication based on user action**. The user should understand they triggered the authentication process in a specific application by clicking a link or a button, or by performing another gesture. Users shouldn't type in credentials in windows that pop up within the operating system with no attached context or actions.
3. **Attempt to [acquire token silently](xref:Microsoft.Identity.Client.AcquireTokenSilentParameterBuilder) first and fall back to [interactive prompt](xref:Microsoft.Identity.Client.AcquireTokenInteractiveParameterBuilder) if that fails**. Customers should only be prompted for interactive authentication if there's an explicit need to reenter credentials or meet a policy requirement.

## Troubleshooting

### "MsalClientException (ErrCode 5376): At least one scope needs to be requested for this authentication flow." error message

This message indicates that you need to request at least one application scope (for example, `user.read`) along with other OIDC scopes (`profile`, `email`, or `offline_access`).

```csharp
var authResult = await pca.AcquireTokenInteractive(new[] { "user.read" })
                 .ExecuteAsync();
```
