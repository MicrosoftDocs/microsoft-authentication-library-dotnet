---
title: Testing applications using MSAL.NET
description: "How to test applications that use MSAL.NET for token acquisition."
ms.date: 03/17/2025
---

# Testing applications using MSAL.NET

## Unit testing

MSAL.NET's API uses the builder pattern heavily. Builders are difficult and tedious to mock. Instead, we recommend that you wrap all your authentication logic behind an interface and mock that in your app.

## End-to-end testing

For end to end testing, you can setup test accounts, test applications or even separate directories. Username and passwords can be deployed via the Continuous Integration pipeline (e.g. secret build variables in Azure DevOps). Another strategy is to keep test credentials in KeyVault and configure the machine that runs the tests to access KeyVault, for example by installing a certificate. Feel free to use MSAL's [strategy for accessing KeyVault](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/master/tests/Microsoft.Identity.Test.LabInfrastructure/KeyVaultSecretsProvider.cs#L112).

Note that once token acquisition occurs, both an Access Token and a Refresh Token are cached. The first has a lifetime of 1h, the latter of several months. When the Access Token expires, MSAL will automatically use the Refresh Token to acquire a new one, without user interaction. You can rely on this behaviour to provision your tests.

If you have Conditional Access configured, automating around it will be difficult. It will be easier to have a manual step that deals with Conditional Access (e.g. MFA), which will add tokens to the MSAL cache and then rely on silent token acquisitions, i.e. rely on a pre-logged in user.

### Web apps

**Strategy 1**: Use Selenium or an equivalent technology to automate your web app. Fetch usernames and password from KeyVault.

Pros: end to end testing with real tokens  

Cons: UI automation is flaky. It's tedious to automate the login screens. Live accounts and "Work and School" have slightly different UI flows.

**Strategy 2**: Use ROPC (Username/Password flow) to get tokens and test only your controllers. Microsoft does not recommend using the ROPC flow in production as it presents some security risks not present in other flows. Use this flow for testing purposes only.

Pros: No ui automation

Cons: Does not work for Live accounts, where ROPC is not supported.

**Strategy 3**: Login manually to prepopulate the token cache. Call `AcquireTokenSilent` to get a fresh access token based on the refresh token **silently**. Refresh tokens are valid for 90 days, but they are also refreshed.

Pros: no ui automation; works for both "Live" and "Work and School" accounts;

Cons: some Conditional Access policies will not work cross machine; some manual setup at first;

Sample showcasing token cache sharing between apps: https://github.com/Azure-Samples/ms-identity-dotnet-advanced-token-cache

### Daemon apps

Daemon apps use pre-deployed secrets (passwords or certificates) to talk to Microsoft Entra ID. You can deploy a secret to your test environment or use the token caching technique to provision your tests. Note that the Client Credential Grant, used by daemon apps, does NOT fetch refresh tokens, just access tokens, which expire in 1h.

### Native client apps

For native clients, there are several approaches to testing:

- Use the [Username / Password](../acquiring-tokens/desktop-mobile/username-password-authentication.md) grant to fetch a token in a non-interactive way. This flow is not recommended in production, but it is reasonable to use it for testing.
- Use a framework, like Appium, that provides an automation interface for both your app and the MSAL created browser.
- MSAL exposes an extensibility point that allows developers to inject their own browser experience. The MSAL team uses this internally to test interactive auth scenarios.

## Library feedback

Please [log issues](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues) or ask questions related to testing. Providing a good test experience is one of the goals of the team.
