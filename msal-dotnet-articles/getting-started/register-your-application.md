---
title: Registering your application with the Microsoft identity platform for developers
---

# Registering your application with the Microsoft identity platform for developers

Before using MSAL.NET you will have to [register your applications](/azure/active-directory/develop/quickstart-register-app) with the Microsoft Identity platform for developers (formerly named Azure AD v2.0)

#### Choosing between ADAL.NET (Azure AD v1.0) and MSAL.NET (Azure AD v2.0)

In most cases you'll want to use MSAL.NET, which is the latest generation of Microsoft authentication libraries. It will allow you to acquire tokens for users signing-in to your application with Azure AD (work and school accounts), Microsoft (personal) accounts (MSA) or [Azure AD B2C](aka.ms/aadb2c). It will also soon support a direct connection to ADFS 2019.

![image](https://user-images.githubusercontent.com/13203188/53400353-f5f35080-39ad-11e9-8270-7e12e34a4ac4.png)

However, you still need to use ADAL.NET if your application needs to sign-in users with earlier versions of Active Directory Federation Services ([ADFS](/windows-server/identity/active-directory-federation-services)). For more details see [ADFS support](https://aka.ms/msal-net-adfs-support).

#### Moving from apps using ADAL.NET (Azure AD v1.0) to using MSAL.NET (Microsoft identity platform)

If you are already familiar with the Azure AD v1.0 endpoint (and ADAL.NET), you might want to read [Comparing the Azure AD v2.0 endpoint with the v1.0 endpoint](/azure/active-directory/develop/active-directory-v2-compare)

See also [ADAL.NET to MSAL.NET](/azure/active-directory/develop/msal-net-migration), which explains how to port an application using ADAL.NET to MSAL.NET
