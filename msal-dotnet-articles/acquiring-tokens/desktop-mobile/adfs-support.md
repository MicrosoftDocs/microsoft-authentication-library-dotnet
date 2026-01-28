---
title: ADFS support in MSAL.NET 
description: Learn about Active Directory Federation Services (ADFS) support in the Microsoft Authentication Library for .NET (MSAL.NET).
author: cilwerner
manager: CelesteDG
ms.author: cwerner
ms.date: 08/24/2023
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer:
ms.topic: concept-article
ms.custom: devx-track-csharp, aaddev, devx-track-dotnet
#Customer intent: As an application developer, I want to learn about AD FS support in MSAL.NET so I can decide if this platform meets my application development needs and requirements.
---

# Active Directory Federation Services support in MSAL.NET

Active Directory Federation Services (ADFS) in Windows Server enables you to add OpenID Connect and OAuth 2.0-based authentication and authorization to applications you are developing. Those applications can authenticate users directly against ADFS. For more information, refer to [AD FS Scenarios for Developers](/windows-server/identity/ad-fs/overview/ad-fs-openid-connect-oauth-flows-scenarios).

Microsoft Authentication Library for .NET (MSAL.NET) supports two scenarios for authenticating against AD FS:

- MSAL.NET talks to Microsoft Entra ID, which itself is *federated* with AD FS.
- MSAL.NET talks **directly** to an ADFS authority. This is only supported in ADFS 2019 and above. One of the scenarios this highlights is [Azure Stack](https://azure.microsoft.com/overview/azure-stack/) support.

## Cases where identity providers are federated with Microsoft Entra ID

MSAL.NET supports talking to Microsoft Entra ID, which itself signs managed (registered in Microsoft Entra ID) or federated (managed by another identity provider, federated through ADFS) users in. MSAL.NET does not differentiate between managed and federated users, as it behaves as if it's directly talking to Microsoft Entra ID in all scenarios.

The [authority](/entra/identity-platform/msal-client-applications) you'll use in this scenario is the usual authority (`common`, `organizations`, or the tenant ID).

### Acquiring a token interactively

When you call the the `AcquireTokenInteractive`,the user experience typically follows the steps:

- The user enters their user ID (or the account or `loginHint` provided as part of the call to <xref:Microsoft.Identity.Client.IPublicClientApplication.AcquireTokenAsync(System.Collections.Generic.IEnumerable{System.String})>).
- Microsoft Entra ID displays a brief "Taking you to your organization's page" UI.
- Microsoft Entra ID redirects the user to the sign-in page of the identity provider (usually customized with the organization branding).

Supported ADFS versions in this scenario are ADFS v2, ADFS v3 (Windows Server 2012 R2), and ADFS v4 (Windows Server 2016).

### Acquiring a token using `AcquireTokenByIntegratedWindowsAuth` or `AcquireTokenByUsernamePassword`

When acquiring a token using the [`AcquireTokenByIntegratedWindowsAuth`](xref:Microsoft.Identity.Client.AcquireTokenByIntegratedWindowsAuthParameterBuilder) or [`AcquireTokenByUsernamePassword`](xref:Microsoft.Identity.Client.AcquireTokenByUsernamePasswordParameterBuilder) methods, MSAL.NET gets the identity provider based on the username. MSAL.NET receives a [SAML 1.1 token](/entra/identity/saas-apps/saml-tutorial) after contacting the identity provider. MSAL.NET then provides the SAML token to Microsoft Entra ID as a user assertion (similar to the [on-behalf-of flow](../web-apps-apis/on-behalf-of-flow.md) to get back a JSON Web Token (JWT).

>[!WARNING]
>**Microsoft does not recommend using the username and password flow**. In most scenarios, more secure alternatives are available and recommended (such as using the [Web Account Manager](wam.md)). This flow requires a very high degree of trust in the application and carries risks that are not present in other flows. You should only use this flow when other more secure flows aren't viable. For more information about why you want to avoid using this grant, see [why Microsoft is working to make passwords a thing of the past](https://news.microsoft.com/features/whats-solution-growing-problem-passwords-says-microsoft/).

## Cases where MSAL connects directly to ADFS

MSAL.NET supports connecting to ADFS 2019, which is OpenID Connect (OIDC) compliant and understands [Proof Key for Code Exchange (PKCE)](https://oauth.net/2/pkce/) and scopes. This support requires that a required update ([KB 4490481](https://support.microsoft.com/help/4490481/windows-10-update-kb4490481)) is applied to Windows Server installation. When connecting directly to ADFS, the authority you'll want to use to build your application is similar to `https://mysite.contoso.com/adfs/`.

Currently, there are no plans to support a direct connection to:

- ADFS on Windows Server 2016, as it doesn't support PKCE and still uses resources (not scopes).
- ADFS v2, which is not OIDC-compliant.

For MSAL.NET we have no plans to support a direct connection to ADFS on Windows Server 2016. When you have upgraded your on-premise systems to ADFS 2019, you'll be able to use MSAL.NET.

MSAL does not support Integrated Windows Authentication (by calling `AcquireTokenByIntegratedWindowsAuth`) for direct connections to ADFS.

## See also

- [Troubleshooting IWA/ADFS Setup](/windows-server/identity/ad-fs/troubleshooting/ad-fs-tshoot-iwa)
- For the federated case, see [Configure Microsoft Entra sign-in behavior for an application by using a Home Realm Discovery policy](/entra/identity/enterprise-apps/configure-authentication-for-federated-users-portal)
