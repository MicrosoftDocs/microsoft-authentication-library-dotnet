---
title: MSAL.NET Public Client
description: "Public client applications are applications that run on devices, desktop computers, or in a web browser, that cannot be trusted to securely store secrets required for authentication."
---

# MSAL.NET Public Client

Public client applications are applications that run on devices, desktop computers, or in a web browser, that cannot be trusted to securely store secrets required for authentication. These applications can only access a web API or service on behalf of the authenticating user and cannot impersonate other users or groups. The reason they can't store a secret is mainly due to the fact that client applications can be reverse-engineered and secrets extracted. The concept is following the definitions included [RFC6749 Section 2.1 - Client Types](https://datatracker.ietf.org/doc/html/rfc6749#section-2.1).

MSAL.NET enables the development of public client applications with the help of <xref:Microsoft.Identity.Client.PublicClientApplicationBuilder> as well as included configuration and functions. Refer to [Token acquisition](/entra/msal/dotnet/acquiring-tokens/overview) for more details.

## Resources

* [Public client and confidential client applications](/azure/active-directory/develop/msal-client-applications)
* [RFC6749 - The OAuth 2.0 Authorization Framework](https://datatracker.ietf.org/doc/html/rfc6749)