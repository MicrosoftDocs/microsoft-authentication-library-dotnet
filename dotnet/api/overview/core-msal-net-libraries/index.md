---
title: Core MSAL.NET Libraries
description: "Core MSAL.NET libraries enable developers to build token acquisition flows into their applications both on the client (e.g., desktop, mobile, and web) as well as on the service sides (e.g., web APIs)."
ms.reviewer: 
---

# Core MSAL.NET Libraries

Core MSAL.NET libraries enable developers to build token acquisition flows into their applications both on the client (e.g., desktop, mobile, and web) as well as on the service sides (e.g., web APIs).

## Public client applications

Public client applications are applications that run on devices, desktop computers, or in a web browser, that cannot be trusted to securely store secrets required for authentication. These applications can only access a web API or service on behalf of the authenticating user and cannot impersonate other users or groups. The reason they can't store a secret is mainly due to the fact that client applications can be reverse-engineered and secrets extracted. The concept is following the definitions included in [RFC6749 Section 2.1 - Client Types](https://datatracker.ietf.org/doc/html/rfc6749#section-2.1).

MSAL.NET enables the development of public client applications with the help of <xref:Microsoft.Identity.Client.PublicClientApplicationBuilder> as well as included configuration and functions.

## Confidential client applications

Confidential client applications are apps that run on servers, such as web apps, web API apps, or service/daemon apps. Their internals are considered difficult to access, and therefore they can keep an application secret secure and out of sight of its users. Confidential clients can hold configuration-time secrets. The concept, just like public client applications, also is following the definitions included in [RFC6749 Section 2.1 - Client Types](https://datatracker.ietf.org/doc/html/rfc6749#section-2.1).

MSAL.NET enables the development of confidential client applications with the help of <xref:Microsoft.Identity.Client.ConfidentialClientApplicationBuilder> as well as included configuration and functions.

## Get started

Refer to [Token acquisition](/entra/msal/dotnet/acquiring-tokens/overview) for more details.

- [`PublicClientApplication`](xref:Microsoft.Identity.Client.PublicClientApplication)
- [`ConfidentialClientApplication`](xref:Microsoft.Identity.Client.ConfidentialClientApplication)
- [`ManagedIdentityApplication`](xref:Microsoft.Identity.Client.ManagedIdentityApplication)

## Resources

- [Public client and confidential client applications](/azure/active-directory/develop/msal-client-applications)
- [RFC6749 - The OAuth 2.0 Authorization Framework](https://datatracker.ietf.org/doc/html/rfc6749)