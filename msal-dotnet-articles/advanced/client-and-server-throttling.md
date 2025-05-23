---
title: Understanding client and server throttling in MSAL.NET
description: "Microsoft Entra ID throttles applications when you call the authentication API too frequently. Learn how to handle this with MSAL.NET."
author: Dickson-Mwendia
manager: 
ms.author: dmwendia
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: conceptual
ms.custom: 
#Customer intent: 
---

# Understanding client and server throttling in MSAL.NET

## Server throttling

Microsoft Entra ID throttles applications when you call the authentication API too frequently. Most often this happens when token caching is not used because:

1. Token caching is not setup correctly (see [Token cache serialization](/azure/active-directory/develop/msal-net-token-cache-serialization)).
2. Not calling <xref:Microsoft.Identity.Client.ClientApplicationBase.AcquireTokenSilent(System.Collections.Generic.IEnumerable{System.String},System.String)> before calling <xref:Microsoft.Identity.Client.IPublicClientApplication.AcquireTokenInteractive(System.Collections.Generic.IEnumerable{System.String})>, <xref:Microsoft.Identity.Client.IPublicClientApplication.AcquireTokenByUsernamePassword(System.Collections.Generic.IEnumerable{System.String},System.String,System.String)>.
3. If you are asking for a scope which does not apply to Microsoft Account (MSA) users, such as `User.ReadBasic.All`, resulting in cache misses.

The server signals throttling in two ways:

- For `client_credentials` grant, i.e., <xref:Microsoft.Identity.Client.ConfidentialClientApplication.AcquireTokenForClient(System.Collections.Generic.IEnumerable{System.String})>, Microsoft Entra ID will reply with `429 Too Many Requests`, with a `Retry-After: 60` header.
- For user-facing calls, Microsoft Entra ID will send a message which results in a <xref:Microsoft.Identity.Client.MsalUiRequiredException> with an `invalid_grant` error code and a message set to `AADSTS50196: The server terminated an operation because it encountered a loop while processing a request`.

## Client throttling

MSAL detects certain conditions where the application should not make repeated calls to Microsoft Entra ID. If a call is made, then a <xref:Microsoft.Identity.Client.MsalThrottledServiceException> or a <xref:Microsoft.Identity.Client.MsalThrottledUiRequiredException> exception is thrown. These are subtypes of <xref:Microsoft.Identity.Client.MsalServiceException>, so this behavior does not introduce a breaking change.

If MSAL would not apply client-side throttling the application would still not be able to acquire tokens as Microsoft Entra ID would throw the error regardless.

## Conditions to get throttled

<a name='azure-ad-is-telling-the-application-to-back-off'></a>

### Microsoft Entra ID is telling the application to back off

If the server is having problems or if an application is requesting tokens too often Microsoft Entra ID will respond with `HTTP 429 (Too Many Requests)` and with `Retry-After` header, `Retry-After X seconds`. The application will see an <xref:Microsoft.Identity.Client.MsalServiceException> with [header details](../advanced/exceptions/retry-policy.md). The throttling state is maintained for X seconds. This limit affects all flows.

The most likely culprit is that you have not setup token caching. See [Token cache serialization in MSAL.NET](/azure/active-directory/develop/msal-net-token-cache-serialization) for details.

<a name='azure-ad-is-having-problems'></a>

### Microsoft Entra ID is having problems

If Microsoft Entra ID is having problems it may respond with a `HTTP 5xx` error code with no `Retry-After` header. The throttling state is maintained for one minute. Affects only public client flows.

### Application is ignoring `MsalUiRequiredException`

MSAL throws <xref:Microsoft.Identity.Client.MsalUiRequiredException> when authentication cannot be resolved silently and the end-user needs to use a browser. This is a common occurrence when a tenant administrator introduced Multi-Factor Authentication (MFA) or when a user's password expires. Retrying the silent authentication cannot succeed. The throttling state is maintained for two minutes. Affects only the <xref:Microsoft.Identity.Client.ClientApplicationBase.AcquireTokenSilent(System.Collections.Generic.IEnumerable{System.String},System.String)> flow.
