---
title: Username and password (ROPC) authentication with MSAL.NET
description: "In your desktop application, you can use the username and password flow to acquire a token silently. No UI is required when using the application."
---

# Username and password (ROPC) authentication with MSAL.NET

In your desktop applications you can use the username and password flow (also known as Resource Owner Password Credentials, or ROPC) to acquire a token silently. No UI is required when using the application.

>[!WARNING]
> The ROPC flow is **not recommended** as the application will be asking a user for their password directly, which is an insecure pattern. For more information about the risks and challenges the ROPC flow poses, refer to ["What’s the solution to the growing problem of passwords? You, says Microsoft"](https://news.microsoft.com/features/whats-solution-growing-problem-passwords-says-microsoft/). The preferred flow for acquiring a token silently on Windows is using the [Windows authentication broker](wam.md). Alternatively, developers can also use the [Device code flow](../desktop-mobile/device-code-flow.md) on devices without access to the web browser.

Although the ROPC flow is useful in limited cases where developers want to provide their own UI for credential acquisition, there are a number of important trade-offs. By using the flow, developers are giving up a number of things:

- Core tenets of modern identity, such as paswordless patterns - if the password gets phished, it can then be replayed.
- Users who need to do Multi-factor Authentication (MFA) won't be able to sign-in, as there are no interaction affordances.
- Single Sign-On (SSO) support.

## Constraints

In addition to the [Integrated Windows Authentication constraints](integrated-windows-authentication.md#iwa-constraints), the following also apply:

- Available starting with MSAL 2.1.0.
- Not compatible with conditional access and multi-factor authentication. As a consequence, if the app runs in an Azure AD tenant where the tenant admin requires multi-factor authentication, the flow cannot be used.
- Only available for work and school accounts and **not** personal Microsoft accounts.
- Available on .NET Framework and .NET/.NET Core, but not for Universal Windows Platform (UWP) applications.

### Authority implications

| Tenant                                                                         | Description                                        | Supports ROPC |
|:-------------------------------------------------------------------------------|:---------------------------------------------------|:--------------|
| `common`                                                                       | Work, school, and personal accounts.               | 🛑 No        |
| `organizations`                                                                | Work and school accounts.                          | ✅ Yes       |
| `consumers`                                                                    | Personal Microsoft accounts.                       | 🛑 No        |
| Specific tenant (GUID or fully-qualified name, like `contoso.onmicrosoft.com`) | Work and school accounts from the specific tenant. | ✅ Yes       |

>[!NOTE]
>To learn more about using the ROPC flow with Azure AD B2C, refer to [Use MSAL.NET to sign in users with social identities](/azure/active-directory/develop/msal-net-aad-b2c-considerations).

## Usage

### Application registration

During the [app registration](https://go.microsoft.com/fwlink/?linkid=2083908), in the **Authentication** section for your application:

- Specify a reply URI.
- Choose **Yes** as the answer to the question **Allow public client flows** (which includes **App collects plaintext password (Resource Owner Password Credential Flow)**).

![Screenshot of the Azure Portal in Microsoft Edge, showing the ROPC flow flag](../../media/ropc-enable-azure-portal.png)

>[!NOTE]
>If your application supports authentication with personal Microsoft accounts, ROPC flow will not be available **even if** your application also supports authentication with work and school accounts.

### Sample code

ROPC flow is only available for public client applications. To use it, developers can leverage the [`PublicClientApplication`](xref:Microsoft.Identity.Client.PublicClientApplication) class, which contains the [`AcquireTokenByUsernamePassword`](xref:Microsoft.Identity.Client.AcquireTokenByUsernamePasswordParameterBuilder) method.

The following sample showcases a simplified use-case:

>[!NOTE]
>Replace `/contoso.com` in the authority URL with your tenant ID or `/organizations`.

```csharp
static async Task GetATokenForGraph()
{
    string authority = "https://login.microsoftonline.com/contoso.com";
    string[] scopes = new string[]
    {
        "user.read"
    };

    IPublicClientApplication app = PublicClientApplicationBuilder.Create(clientId).WithAuthority(authority).Build();

    var accounts = await app.GetAccountsAsync();
    AuthenticationResult result = null;

    if (accounts.Any())
    {
        result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
    }
    else
    {
        try
        {
            var securePassword = new string();
            foreach(char c in "dummy") // you should fetch the password
            securePassword.AppendChar(c); // keystroke by keystroke
            result = await app.AcquireTokenByUsernamePassword(scopes, "joe@contoso.com", securePassword).ExecuteAsync();
        }
        catch (MsalException)
        {
            // See details below
        }
    }
    Console.WriteLine(result.Account.Username);
}
```

The following sample presents the most current use-case, with explanations of exceptions that can be thrown and their mitigations:

```csharp
static async Task GetATokenForGraph()
{
    string authority = "https://login.microsoftonline.com/contoso.com";
    string[] scopes = new string[]
    {
        "user.read"
    };

    IPublicClientApplication app;
    app = PublicClientApplicationBuilder.Create(clientId).WithAuthority(authority).Build();

    var accounts = await app.GetAccountsAsync();
    AuthenticationResult result = null;

    if (accounts.Any())
    {
        result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAync();
    }
    else
    {
        try
        {
            var securePassword = new SecureString();
            foreach(char c in "dummy") // you should fetch the password keystroke
            securePassword.AppendChar(c); // by keystroke
            result = await app.AcquireTokenByUsernamePassword(scopes, "joe@contoso.com", securePassword).ExecuteAsync();
        }
        catch (MsalUiRequiredException ex) when(ex.Message.Contains("AADSTS65001"))
        {
            // Here are the kind of error messages you could have, and possible mitigations
            // ------------------------------------------------------------------------
            // MsalUiRequiredException: AADSTS65001: The user or administrator has not consented to use the application
            // with ID '{appId}' named '{appName}'. Send an interactive authorization request for this user and resource.
            // Mitigation: you need to get user consent first. This can be done either statically (through the portal), 
            /// or dynamically (but this requires an interaction with Azure AD, which is not possible with 
            // the username/password flow)
            // Statically: in the portal by doing the following in the "API permissions" tab of the application registration:
            // 1. Click "Add a permission" and add all the delegated permissions corresponding to the scopes you want (for instance
            // User.Read and User.ReadBasic.All)
            // 2. Click "Grant/revoke admin consent for <tenant>") and click "yes".
            // Dynamically, if you are not using .NET Core (which does not have any Web UI) by 
            // calling (once only) AcquireTokenInteractive.
            // remember that Username/password is for public client applications that is desktop/mobile applications.
            // If you are using .NET core or don't want to call AcquireTokenInteractive, you might want to:
            // - use device code flow (See https://aka.ms/msal-net-device-code-flow)
            // - or suggest the user to navigate to a URL to consent: https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&scope=user.read
            // ------------------------------------------------------------------------
            // ------------------------------------------------------------------------
            // ErrorCode: invalid_grant
            // SubError: basic_action
            // MsalUiRequiredException: AADSTS50079: The user is required to use multi-factor authentication.
            // The tenant admin for your organization has chosen to oblige users to perform multi-factor authentication.
            // Mitigation: none for this flow
            // Your application cannot use the Username/Password grant.
            // Like in the previous case, you might want to use an interactive flow (AcquireTokenInteractive()), 
            // or Device Code Flow instead.
            // Note this is one of the reason why using username/password is not recommended;
            // ------------------------------------------------------------------------
            // ------------------------------------------------------------------------
            // ex.ErrorCode: invalid_grant
            // subError: null
            // Message = "AADSTS70002: Error validating credentials.
            // AADSTS50126: Invalid username or password
            // In the case of a managed user (user from an Azure AD tenant opposed to a
            // federated user, which would be owned
            // in another IdP through ADFS), the user has entered the wrong password
            // Mitigation: ask the user to re-enter the password
            // ------------------------------------------------------------------------
            // ------------------------------------------------------------------------
            // ex.ErrorCode: invalid_grant
            // subError: null
            // MsalServiceException: ADSTS50034: To sign into this application the account must be added to 
            // the {domainName} directory.
            // or The user account does not exist in the {domainName} directory. To sign into this application, 
            // the account must be added to the directory.
            // The user was not found in the directory
            // Explanation: wrong username
            // Mitigation: ask the user to re-enter the username.
            // ------------------------------------------------------------------------
        }
        catch (MsalServiceException ex) when(ex.ErrorCode == "invalid_request")
        {
            // ------------------------------------------------------------------------
            // AADSTS90010: The grant type is not supported over the /common or /consumers endpoints. 
            // Please use the /organizations or tenant-specific endpoint.
            // you used common.
            // Mitigation: as explained in the message from Azure AD, the authority you use in the application needs 
            // to be tenanted or otherwise "organizations". change the
            // "Tenant": property in the appsettings.json to be a GUID (tenant Id), or domain name (contoso.com) 
            // if such a domain is registered with your tenant
            // or "organizations", if you want this application to sign-in users in any Work and School accounts.
            // ------------------------------------------------------------------------
        }
        catch (MsalServiceException ex) when(ex.ErrorCode == "unauthorized_client")
        {
            // ------------------------------------------------------------------------
            // AADSTS700016: Application with identifier '{clientId}' was not found in the directory '{domain}'.
            // This can happen if the application has not been installed by the administrator of the tenant or consented 
            // to by any user in the tenant.
            // You may have sent your authentication request to the wrong tenant
            // Cause: The clientId in the appsettings.json might be wrong
            // Mitigation: check the clientId and the app registration
            // ------------------------------------------------------------------------
        }
        catch (MsalServiceException ex) when(ex.ErrorCode == "invalid_client")
        {
            // ------------------------------------------------------------------------
            // AADSTS70002: The request body must contain the following parameter: 'client_secret or client_assertion'.
            // Explanation: this can happen if your application was not registered as a public client application in Azure AD
            // Mitigation: in the Azure portal, edit the manifest for your application and set the `allowPublicClient` to `true`
            // ------------------------------------------------------------------------
        }
        catch (MsalServiceException)
        {
            throw;
        }
        catch (MsalClientException ex) when(ex.ErrorCode == "unknown_user_type")
        {
            // Message = "Unsupported User Type 'Unknown'. Please see https://aka.ms/msal-net-up"
            // The user is not recognized as a managed user, or a federated user. Azure AD was not
            // able to identify the IdP that needs to process the user
            throw new ArgumentException("U/P: Wrong username", ex);
        }
        catch (MsalClientException ex) when(ex.ErrorCode == "user_realm_discovery_failed")
        {
            // The user is not recognized as a managed user, or a federated user. Azure AD was not
            // able to identify the IdP that needs to process the user. That's for instance the case
            // if you use a phone number
            throw new ArgumentException("U/P: Wrong username", ex);
        }
        catch (MsalClientException ex) when(ex.ErrorCode == "unknown_user")
        {
            // the username was probably empty
            // ex.Message = "Could not identify the user logged into the OS. See http://aka.ms/msal-net-iwa for details."
            throw new ArgumentException("U/P: Wrong username", ex);
        }
        catch (MsalClientException ex) when(ex.ErrorCode == "parsing_wstrust_response_failed")
        {
            // ------------------------------------------------------------------------
            // In the case of a Federated user (that is owned by a federated IdP, as opposed to a managed user owned in an Azure AD tenant)
            // ID3242: The security token could not be authenticated or authorized.
            // The user does not exist or has entered the wrong password
            // ------------------------------------------------------------------------
        }
    }
    Console.WriteLine(result.Account.Username);
}
```

## Protocol documentation

See [Azure Active Directory v2.0 and the OAuth 2.0 resource owner password credential](/azure/active-directory/develop/v2-oauth-ropc) to learn more about the underlying protocol.

## End-to-end samples

| Sample | Platform | Description |
| ------ | -------- | ----------- |
| [active-directory-dotnetcore-console-up-v2](https://github.com/Azure-Samples/active-directory-dotnetcore-console-up-v2) | Console (.NET Core) | .NET Core console application letting a user signed-in with the Azure AD v2.0 endpoint using username and password to acquire a token for Microsoft Graph. ! [Console app topology](../../media/console-app-topology.png) |
