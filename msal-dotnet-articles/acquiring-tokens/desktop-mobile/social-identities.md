---
title: Using MSAL.NET to sign-in users with social identities
description: "You can use MSAL.NET to sign-in users with social identities by using Azure AD B2C. Azure AD B2C is built around the notion of policies. In MSAL.NET, specifying a policy translates to providing an authority."
services: active-directory
author: henrymbuguakiarie
manager: CelesteDG

ms.service: active-directory
ms.subservice: develop
ms.topic: conceptual
ms.workload: identity
ms.date: 08/24/2023
ms.author: dmwendia
ms.reviewer: ddelimarsky, saeeda, jeferrie
ms.custom: devx-track-csharp, aaddev, devx-track-dotnet
# Customer intent: As an application developer, I want to learn about specific considerations when using Azure AD B2C and MSAL.NET so I can decide if this platform meets my application development needs and requirements.
---

# Using MSAL.NET to sign-in users with social identities

You can use MSAL.NET to sign-in users with social identities by using [Azure AD B2C](/azure/active-directory-b2c/overview). Azure AD B2C is built around the notion of policies. In MSAL.NET, specifying a policy translates to providing an authority.

- When you instantiate the public client application, you need to specify the policy in authority
- When you want to apply a policy, you need to call an override of `AcquireTokenInteractive` containing an `authority` parameter

## Authority for an Azure AD B2C tenant and policy

The authority to use is `https://login.microsoftonline.com/tfp/{tenant}/{policyName}` where:

- `tenant` is the name of the Azure AD B2C tenant, 
- `policyName` the name of the policy to apply (for instance "b2c_1_susi" for sign-in/sign-up).

The current guidance from B2C is to use `b2clogin.com` as the authority. For example, `$"https://{your-tenant-name}.b2clogin.com/tfp/{your-tenant-ID}/{policyname}"`. For more information, see [Set redirect URLs to b2clogin.com.](/azure/active-directory-b2c/b2clogin).

```csharp
// Azure AD B2C Coordinates
public static string Tenant = "fabrikamb2c.onmicrosoft.com";
public static string ClientID = "90c0fe63-bcf2-44d5-8fb7-b8bbc0b29dc6";
public static string PolicySignUpSignIn = "b2c_1_susi";
public static string PolicyEditProfile = "b2c_1_edit_profile";
public static string PolicyResetPassword = "b2c_1_reset";

public static string AuthorityBase = $"https://fabrikamb2c.b2clogin.com/tfp/{Tenant}/";
public static string Authority = $"{AuthorityBase}{PolicySignUpSignIn}";
public static string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}";
public static string AuthorityPasswordReset = $"{AuthorityBase}{PolicyResetPassword}";
```

### Instantiating the application

When building the application, you need to provide, as usual, the authority, built as above

```csharp
application = PublicClientApplicationBuilder.Create(ClientID)
               .WithB2CAuthority(Authority)
               .Build();
```

## Acquiring a token to apply a policy

> Starting with MSAL .NET 4.15.0, developers will no longer have to write their own cache filtering logic.

In Azure AD B2C, each policy, or user flow, is a separate authorization server. They issue their own tokens. So a token acquired using the `b2c_1_editprofile` user flow will not work with a resource protected behind a `b2c_1_susi` user flow. Therefore, when calling a protected API, application developers must let MSAL know which token to use from the cache, based on the user flow that will be targeted.

Acquiring a token for an Azure AD B2C protected API in a public client application requires you to use:

- The override of GetAccountsAsync() with a user flow before calling AcquireTokenSilent,
- The overrides AcquireTokenInteractive with a B2C authority:

```csharp
IEnumerable<IAccount> accounts = await application.GetAccountsAsync(B2CConstants.PolicySignUpSignIn);
AuthenticationResult ar = await application.AcquireTokenInteractive(B2CConstants.Scopes)
                                           .WithAccount(accounts.FirstOrDefault())
                                           .ExecuteAsync();

```

In versions > 4.15.0, developers had to write their own cache filtering logic. This is no longer the case in >= 4.15.0, as developers only need to specify the policy, or user flow, and MSAL will return the corresponding account for that specific user flow.

<table>
<tr><td>In MSAL.NET, you only write:</td><td>In MSAL < 4.15.0, you had to write:</td></tr>
<tr><td>

```csharp
var accounts = await app.GetAccountsAsync(App.PolicySignUpSignIn);
```

</td><td>

```csharp
private IAccount GetAccountByPolicy(IEnumerable<IAccount> accounts, string policy)
{
 foreach (var account in accounts)
 {
  string userIdentifier = account.HomeAccountId.ObjectId.Split('.')[0];
  if (userIdentifier.EndsWith(policy.ToLower()))
   return account;
 }
 return null;
}
```

</td>
</table>

Applying a policy (for instance letting the end user edit their profile or reset their password) is currently done by calling AcquireTokenInteractive.
> Note that in the case of these two policies you don't use the returned token / authentication result.

## Special case of EditProfile and ResetPassword policies

When you want to provide an experience where your end users sign-in with a social identity, and then edit their profile you want to apply the B2C EditProfile policy. The way to do this, is by calling `AcquireTokenInteractive` with
the specific authority for that policy and a Prompt set to `Prompt.NoPrompt` to avoid the account selection dialog to be displayed (as the user is already signed-in)

```csharp
private async void EditProfileButton_Click(object sender, RoutedEventArgs e)
{
 IEnumerable<IAccount> accounts = await app.GetAccountsAsync();
 try
 {
  var authResult = await app.AcquireToken(scopes:App.ApiScopes)
                               .WithAccount(GetUserByPolicy(accounts, App.PolicyEditProfile)),
                               .WithPrompt(Prompt.NoPrompt),
                               .WithB2CAuthority(App.AuthorityEditProfile)
                               .ExecuteAsync();
  DisplayBasicTokenInfo(authResult);
 }
 catch
 {
  . . .
}
```

Now in preview, is [Self-service password reset](/azure/active-directory-b2c/add-password-reset-policy?pivots=b2c-user-flow#self-service-password-reset-recommended), which means the new password reset experience is now part of the sign-in or sign-up/sign-in (Recommended) user flows. This also means, once you have enabled this preview feature, you can remove this section of code:

```csharp
 if (ex.Message.Contains("AADB2C90118"))
{
       authResult = await app.AcquireTokenInteractive(App.ApiScopes)
              .WithParentActivityOrWindow(new WindowInteropHelper(this).Handle)
              .WithPrompt(Prompt.SelectAccount)
              .WithB2CAuthority(App.AuthorityResetPassword)
              .ExecuteAsync();
}
```

Or whichever special logic you were doing to process the `AADB2C90118` error.

## Resource Owner Password Credentials (ROPC) With B2C

For more details on the ROPC flow, please see this [documentation](./username-password-authentication.md).

### This flow is not recommended

This flow is **not recommended** because your application asking a user for their password is not secure. For more information about this problem, see [this article](https://news.microsoft.com/features/whats-solution-growing-problem-passwords-says-microsoft/).

By using username/password you are giving-up a number of things:

- Core tenants of modern identity: password gets fished, replayed. Because we have this concept of a share secret that can be intercepted. This is incompatible with passwordless.
- Users who need to do MFA won't be able to sign-in (as there is no interaction)
- Users won't be able to do single sign-on

### Configure the ROPC flow in AzureAD B2C

In your AzureAD B2C tenant, create a new user flow and select **Sign in using ROPC**. This will enable the ROPC policy for your tenant. See [Configure the resource owner password credentials flow](/azure/active-directory-b2c/configure-ropc) for more details.

`IPublicClientApplication` contains a method called `AcquireTokenByUsernamePassword`:

```csharp
AcquireTokenByUsernamePassword(
            IEnumerable<string> scopes,
            string username,
            SecureString password)
```

This method takes as parameters:

- The `scopes` to request an access token for
- A username
- A SecureString password for the user

Remember to use the authority which contains the ROPC policy.

### Limitations of the ROPC flow

This flow **only works for local accounts** (where you register with B2C using an email or username). This flow does not work if federating to any of the IdPs supported by B2C (Facebook, Google, etc.).

## Google Auth and Embedded Webview

If you are a B2C developer using Google as an identity provider we recommend you use the system browser, as Google does not allow [authentication from embedded webviews](https://developers.googleblog.com/2016/08/modernizing-oauth-interactions-in-native-apps.html). Currently, `login.microsoftonline.com` is a trusted authority with Google. Using this authority will work with embedded webview. However using `b2clogin.com` is not a trusted authority with Google, so users will not be able to authenticate.

We will provide an update to the wiki and this [issue](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/688) if things change.

## Caching with B2C in MSAL.NET

### Known issue with Azure B2C

MSAL.Net supports a [token cache](/dotnet/api/microsoft.identity.client.tokencache). The token caching key is based on the claims returned by the Identity Provider. Currently MSAL.Net needs two claims to build a token cache key:

1. `tid` which is the Azure AD Tenant Id
1. `preferred_username`

Both these claims are missing in many of the Azure AD B2C scenarios.

The customer impact is that when trying to display the username field, are you getting "Missing from the token response" as the value? If so, this is because B2C does not return a value in the IdToken for the preferred_username because of limitations with the social accounts and external identity providers (IdPs). Azure AD returns a value for preferred_username because it knows who the user is, but for B2C, because the user can sign in with a local account, Facebook, Google, GitHub, etc...there is not a consistent value for B2C to use for preferred_username. To unblock MSAL from rolling out cache compatibility with ADAL, we decided to use "Missing from the token response" on our end when dealing with the B2C accounts when the IdToken returns nothing for preferred_username. MSAL must return a value for preferred_username to maintain cache compatibility across libraries.

### Workarounds

#### Mitigation of the lack of `tid`

The suggested workaround  is to use the [Caching by Policy](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/AAD-B2C-specifics#acquiring-a-token-to-apply-a-policy).

Alternatively, you can use the `tid` claim, if you are using the [B2C custom policies](/azure/active-directory-b2c/tutorial-create-user-flows?pivots=b2c-custom-policy), because it provides the capability to return additional claims to the application. To learn more about [Claims Transformation](/azure/active-directory-b2c/claims-transformation-technical-profile).

#### Mitigation for "Missing from the token response"

One option is to use the "name" claim as the preferred username. The process is generally mentioned in this [B2C doc](/azure/active-directory-b2c/active-directory-b2c-reference-policies#frequently-asked-questions):

> "In the Return claim column, choose the claims you want returned in the authorization tokens sent back to your application after a successful profile editing experience. For example, select Display Name, Postal Code.”

## Customizing the UI

[Customize the user interface with Azure AD B2C](/azure/active-directory-b2c/customize-ui-overview).

## Samples illustrating acquiring tokens interactively with MSAL.NET for B2C applications

Sample | Platform | Description
------ | -------- | -----------
[active-directory-b2c-xamarin-native](https://github.com/Azure-Samples/active-directory-b2c-xamarin-native) | Xamarin iOS, Xamarin Android, UWP | A simple Xamarin Forms app showcasing how to use MSAL.NET to authenticate users via Azure Active Directory B2C, and access a Web API with the resulting tokens.
