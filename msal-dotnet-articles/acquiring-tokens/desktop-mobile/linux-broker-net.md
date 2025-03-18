---
# Required metadata
# For more information, see https://review.learn.microsoft.com/en-us/help/platform/learn-editor-add-metadata?branch=main
# For valid values of ms.service, ms.prod, and ms.topic, see https://review.learn.microsoft.com/en-us/help/platform/metadata-taxonomies?branch=main

title: Using MSAL.NET with Linux
description: MSAL is able to call the Microsoft Single Sign-on for Linux broker, a Linux component as a dependency of Intune. This component acts as an authentication broker allowing the users of your app to benefit from integration with accounts known to the Linux Broker.
author:      ploegert # GitHub alias
ms.author:   jploegert # Microsoft alias
ms.service: msal
ms.topic: how-to
ms.date:     03/18/2025
ms.subservice: msal-dotnet
---

# Using MSAL .NET with an Authentication Broker on Linux


> [!NOTE]
> Microsoft Single Sign-on for Linux authentication broker support is introduced with `MSAL` version v4.69.1.

Using an authentication broker on Linux enables you to simplify how your users authenticate with Microsoft Entra ID from your application, as well as take advantage of future functionality that protects Microsoft Entra ID refresh tokens from exfiltration and misuse.

Authentication brokers are **not** pre-installed on Linux but is bundled as a dependency of applications developed by Microsoft, such as [Company Portal](/mem/intune-service/user-help/enroll-device-linux). These applications are usually installed when a Linux computer is enrolled in a company's device fleet via an endpoint management solution like [Microsoft Intune](/mem/intune/fundamentals/what-is-intune). To learn more about Linux device set up with the Microsoft Identity Platform, refer to [Microsoft Enterprise SSO plug-in for Apple devices](/entra/identity-platform/apple-sso-plugin).

## Usage

To use the broker, you will need to install the broker-related packages in addition to the core MSAL from PyPI:

```bash
pip install msal[broker]>=1.31,<2
```

>[!IMPORTANT]
>If broker-related packages are not installed and you will try to use the authentication broker, you will get an error: `ImportError: You need to install dependency by: pip install "msal[broker]>=1.31,<2"`.

Typically, on macOS your [public client](/entra/identity-platform/msal-client-applications) Python applications would [acquire tokens](../getting-started/acquiring-tokens.md) via the system browser. To use authentication brokers installed on a macOS system instead, you will need to pass an additional argument in the `PublicClientApplication` constructor - `enable_broker_on_mac`:

```python
from msal import PublicClientApplication
 
app = PublicClientApplication(
    "CLIENT_ID",
    authority="https://login.microsoftonline.com/common",
    enable_broker_on_mac =True)
```

>[!IMPORTANT]
>If you are writing a cross-platform application, you will also need to use `enable_broker_on_windows`, as outlined in the [Using MSAL Python with Web Account Manager](wam.md) article.

In addition to the constructor change, your application needs to support broker-specific redirect URIs. For _unsigned_ applications, the URI is:

```text
msauth.com.msauth.unsignedapp://auth 
```

For signed applications, the redirect URI should be:

```text
msauth.BUNDLE_ID://auth
```

If the redirect URIs are not correctly set in the app configuration within the Entra portal, you will receive error like this: 

```text
Error detected... 
tag=508170375
context=AADSTS50011 Description: (pii), Domain: MSAIMSIDOAuthErrorDomain.Error was thrown in location: Broker 
errorCode=-51411 
status=Response_Status.Status_Unexpected 
```

Once configured, you can call `acquire_token_interactive` to acquire a token.

```python
result = app.acquire_token_interactive(["User.ReadBasic.All"],
                    parent_window_handle=app.CONSOLE_WINDOW_HANDLE)
```

>[!NOTE]
>The `parent_window_handle` parameter is required even though on macOS it is not used. For GUI applications, the login prompt location will be determined ad-hoc and currently cannot be bound to a specific window. In a future update, this parameter will be used to determine the _actual_ parent window.

## Token caching

The authentication broker handles refresh and access token caching. You do not need to set up custom caching.