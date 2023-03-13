---
title: ADFS Support in MSAL.NET
---

# ADFS Support in MSAL.NET

There are two cases:

- MSAL.NET talks to Azure Active Directory, which itself is **federated** with other identity providers (IdPs). In the case we are interested in the federation happens through ADFS.
- MSAL.NET talks **directly** to an ADFS authority. This is only supported from AD FS 2019 and above. One of the scenarios this highlights is [Azure Stack](https://azure.microsoft.com/overview/azure-stack/) support.

## Cases where identity providers are federated with Azure AD

MSAL.NET supports talking to Azure AD, which itself signs-in managed users (users managed in Azure AD), or federated users (users managed by another identity provider, which, in the case we are interested is federated through ADFS). MSAL.NET does not know about the fact that users are federated. As far as it’s concerned, it talks to Azure AD.

The [authority](/azure/active-directory/develop/msal-client-applications) you'll use in the case is the usual authority (common, organizations, or tenanted)

### Acquiring a token interactively

When you call <xref:microsoft.identity.client.ipublicclientapplication.acquiretokeninteractive>, in term of user experience:

- the user enter their upn (or the account or `loginHint` is provided part of the call to <xref:microsoft.identity.client.ipublicclientapplication.acquiretokenasync>)
- Azure AD displays briefly "Taking you to your organization's page"
- and then redirects the user is to the sign-in page of the identity provider (usually customized with the logo of the organization)

Supported ADFS versions in this federated scenario are ADFS v2 , ADFS v3 (Windows Server 2012 R2) and ADFS v4 (ADFS 2016)

### Acquiring a token using `AcquireTokenByIntegratedAuthentication` or `AcquireTokenByUsernamePassword`

In that case, from the username, MSAL.NET goes to Azure Active Directory (`userrealm` endpoint) passing the username, and it gets information about the IdP to contact. It does, passing the username (and the password in the case of <xref:microsoft.identity.client.ipublicclientapplication.acquiretokenbyusernamepassword>) and receives a [SAML 1.1 token](/azure/active-directory/develop/reference-saml-tokens), which it provides to Azure Active Directory as a user assertion (similar to the [on-behalf-of](../web-apps-apis/on-behalf-of-flow.md) flow) to get back a JWT.

## Case where MSAL connects directly to ADFS

In that case the authority you'll want to use to build your application is something like `https://somesite.contoso.com/adfs/`

MSAL.NET supports ADFS 2019 (PR is [ADFS Compatibility with MSAL #834](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/pull/834)), which understands PKCE and scopes, after a service pack [KB 4490481](https://support.microsoft.com/help/4490481/windows-10-update-kb4490481) is applied to Windows Server.

However for MSAL.NET we have no plans to support a direct connection to ADFS 2016 (it does not support PKCE and still uses resources, not scope). If you need to support today scenarios requiring a direct connection to ADFS 2016, please use the latest version of ADAL. When you have upgraded your on-premise system to ADFS 2019, you'll be able to use MSAL.NET.

MSAL does not support Integrated Windows Authentication (by calling AcquireTokenByIntegratedWindowsAuth) directly to ADFS.

## See also

- [Troubleshooting IWA/ADFS Setup](/windows-server/identity/ad-fs/troubleshooting/ad-fs-tshoot-iwa)
- For the federated case, see [Configure Azure Active Directory sign in behavior for an application by using a Home Realm Discovery policy](/azure/active-directory/manage-apps/configure-authentication-for-federated-users-portal)
