---
title: Token Acquisition
description: "Learn how to acquire security tokens in public and confidential client applications using MSAL.NET."
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 05/20/2024
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: reference
ms.custom: devx-track-csharp, aaddev
#Customer intent: 
# Customer intent: As an application developer, I want to learn how to acquire security tokens in public and confidential client applications using MSAL.NET
---

# Token acquisition

## Application types

As explained in [Scenarios](../getting-started/scenarios.md), there are many ways of acquiring a token with MSAL.NET. Some require interaction and others are completely transparent to the user. The approach used to acquire a token is different depending on whether the developer is building a public client (desktop or mobile) or a confidential client application (web app, web API, or daemon like a Windows service). Public clients generally require user interaction while confidential clients rely on pre-provisioned credentials, like certificates and secrets.

## Token caching

For both public and confidential client applications, MSAL.NET supports adding a token cache that preserves authentication and refresh tokens, as well as proactively refreshes those on an as-needed basis. For details, see [Token cache serialization in MSAL.NET](../how-to/token-cache-serialization.md).

For .NET desktop applications (.NET, .NET Framework, and .NET Core) the application needs to handle the token cache serialization and storage directly; however, helper classes are available to help simplify the process.

## Token acquisition methods

### Public client applications

- Will often [acquire token interactively](./desktop-mobile/acquiring-tokens-interactively.md), having the user sign-in.
- It's also possible for a desktop application running on a Windows machine joined to a domain or to Microsoft Entra ID to [use Integrated Windows Authentication (IWA/Kerberos)](./desktop-mobile/integrated-windows-authentication.md) to acquire a token silently.
  - Keep in mind that the IWA approach is **not recommended**. A more secure approach using the [Web Account Manager (WAM)](../acquiring-tokens/desktop-mobile/wam.md) is available.
- For .NET Framework desktop applications, in limited scenarios it's possible to [get a token with a username and password](./desktop-mobile/username-password-authentication.md). Due to security considerations, this approach is not recommended.
- In applications running on devices which don't have a web browser, a token can be acquired with the help of the [device code flow](./desktop-mobile/device-code-flow.md), which provides the application user with a URL and a code. The user will subsequently go to a web browser on another device, enter the code, and sign in. The authenticating device will then poll Microsoft Entra ID services until it receives confirmation of a successful sign in and an access token.

The following table summarizes the available approaches to acquire tokens in public client applications:

| Operating system      | Platform        | App type | [Interactive](./desktop-mobile/acquiring-tokens-interactively.md) | [IWA](./desktop-mobile/integrated-windows-authentication.md) | [Device Code](./desktop-mobile/device-code-flow.md) |
|:----------------------|:----------------|:---------|:------------------------------------------------------------------|:-------------------------------------------------------------|:----------------------------------------------------|
| Windows (desktop)     | .NET            | Desktop (WPF, Windows Forms, Console) | ✅ | ✅ | ✅ |
| Android               | .NET MAUI       | Mobile                                | ✅ | ❌ | ❌ |
| iOS                   | .NET MAUI       | Mobile                                | ✅ | ❌ | ✅ |
| macOS, Linux, Windows | .NET Core       | Console                               | N/A see [Using web browsers](../acquiring-tokens/using-web-browsers.md)	| ✅	| ✅ |

### Confidential client applications

- Acquires token **for the application itself**, not for a user. Token acquisition is done with the help of [client credentials](./web-apps-apis/client-credential-flows.md). This flow is useful for syncing tools or tools which process data or user information without a specific identity attached to it.
- For web APIs calling an API on behalf of a user, developers can use [On Behalf Of flow](./web-apps-apis/on-behalf-of-flow.md). The application itself will use client credentials to acquire a token based on a user assertion (e.g., [SAML](/entra/identity-platform/single-sign-on-saml-protocol) or a [JWT](/entra/identity-platform/security-tokens#json-web-tokens-and-claims)). This flow can be used for applications which need to access resources of a particular user in service-to-service calls.
- **For web apps**, token acquisition is done using an [authorization code](./web-apps-apis/authorization-codes.md) after signing the user in through the authorization request URL. This is typically the mechanism used by an application which lets the user sign-in using [OpenID Connect](https://openid.net/developers/how-connect-works/) and then accesses web APIs on behalf of this particular user.

The following table summarizes the ways to acquire tokens in confidential client applications:

| Operating system      | Platform       | App type | [Client Credential](./web-apps-apis/client-credential-flows.md) | [On-Behalf-Of](./web-apps-apis/on-behalf-of-flow.md) | [Auth Code](./web-apps-apis/authorization-codes.md)
|:----------------------|:---------------|:---------|:----------------------------------------------------------------|:-----------------------------------------------------|:---------------------------------------------------
| Windows               | .NET Framework | Web app                  | ✅ | ❌ | ✅ |
| Windows, macOS, Linux | ASP.NET Core   | Web app                  | ✅ | ❌ | ✅ |
| Windows               | .NET Framework | Web API                  | ✅ | ✅ | ❌ |
| Windows, macOS, Linux | ASP.NET Core   | Web API                  | ✅ | ✅ | ❌ |
| Windows               | .NET Framework | Daemon (Windows service) | ✅ | ❌ | ❌ |
| Windows, macOS, Linux | .NET Core      | Daemon                   | ✅ | ❌ | ❌ |

### Pattern to acquire tokens in MSAL.NET

All the Acquire Token methods in MSAL.NET have the following pattern:

- From the application, you call the AcquireToken*XXX* method corresponding to the flow you want to use, passing the mandatory parameters for this flow (in general flow)
- This returns a command builder, on which you can add optional parameters using .With*YYY* methods
- Then you call ExecuteAsync() to get your authentication result.

Here is the pattern:

```csharp
AuthenticationResult result = app.AcquireTokenXXX(mandatory-parameters)
 .WithYYYParameter(optional-parameter)
 .ExecuteAsync();
```

## `AuthenticationResult` properties in MSAL.NET

In all cases above, methods to acquire tokens return an `AuthenticationResult` (or in the case of the async methods a `Task<AuthenticationResult>`.

In MSAL.NET, AuthenticationResult exposes:

- `AccessToken` for the Web API to access resources. This is a string, usually a base64 encoded JWT but the client should never look inside the access token. The format isn't guaranteed to remain stable, and it can be encrypted for the resource. People writing code depending on access token content on the client is one of the biggest sources of errors and client logic breaks 
- `IdToken` for the user (this is a JWT)
- `ExpiresOn` tells the date/time when the token expires
- `TenantId` contains the tenant in which the user was found. Note that in the case of guest users (Microsoft Entra B2B scenarios), the TenantId is the guest tenant, not the unique tenant.
When the token is delivered in the name of a user, `AuthenticationResult` also contains information about this user. For confidential client flows where tokens are requested with no user (for the application), this User information is null.
- The `Scopes` for which the token was issued (See [Scopes not resources](/azure/active-directory/develop/msal-net-differences-adal-net))
- The unique Id for the user.

## `IAccount`

MSAL.NET defines the notion of Account (through the `IAccount` interface). This breaking change provides the right semantics: the fact that the same user can have several accounts, in different Microsoft Entra directories. Also MSAL.NET provides better information in the case of guest scenarios, as home account information is provided.
The following diagram shows the structure of the `IAccount` interface:

![image](../media/authenticationresult-graph.png)

The `AccountId` class identifies an account in a specific tenant. It has the following properties:

Property | Description
-------- | -----------
`TenantId` | A string representation for a GUID, which is the ID of the tenant where the account resides
`ObjectId` | A string representation for a GUID which is the ID of the user who owns the account in the tenant
`Identifier` | Unique identifier for the account (this is the concatenation of `ObjectId` and `TenantId` separated by a comma and are not base64 encoded)

The `IAccount` interface represents information about a single account. The same user can be present in different tenants, that is, a user can have multiple accounts. Its members are:

Property | Description
--- | ----
`Username` | A string containing the displayable value in UserPrincipalName (UPN) format, for example, john.doe@contoso.com. This can be null, whereas the HomeAccountId and HomeAccountId.Identifier won’t be null. This property replaces the `DisplayableId` property of `IUser` in previous versions of MSAL.NET.
`Environment` | A string containing the identity provider for this account, for example, `login.microsoftonline.com`. This property replaces the `IdentityProvider` property of `IUser`, except that `IdentityProvider` also had information about the tenant (in addition to the cloud environment), whereas here this is only the host.
`HomeAccountId` | AccountId of the home account for the user. This uniquely identifies the user across Microsoft Entra tenants.
