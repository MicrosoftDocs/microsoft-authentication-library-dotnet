> This is for MSAL.NET 2.x. if you are interested in MSAL.NET 3.x see [Username Password in MSAL.NET 3.x](Username-Password-Authentication)

## Username / Password flow

In your desktop application, you can use the Username/Password flow to acquire a token silently. No UI is required when using the application.

### This flow is not recommended

This flow is **not recommended** because your application asking user for their password is not secure. For more information about this problem, see [this article](https://news.microsoft.com/features/whats-solution-growing-problem-passwords-says-microsoft/). The preferred flow for acquiring a token silently on Windows domain joined machines is [Integrated Windows Authentication](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Integrated-Windows-Authentication).

> If you want to use Username/password, you should really think about how to move away from it. By using username/password you are giving-up a number of things:
> - core tenants of modern identity: password get fished, replayed. Because we have this concept of a share secret that can be intercepted.
> This is incompatible with passwordless.
> - users who need to do MFA won't be able to sign-in (as there is no inMteraction)
> - Users won't be able to do single sign-on

### Constraints

Apart from the [Integrated Windows Authentication constraints](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Integrated-Windows-Authentication#constraints), the following constraints also apply:

- Available starting with MSAL 2.1.0
- The Username/Password flow is not compatible with conditional access and multi-factor authentication: As a consequence, if your app runs in an Azure AD tenant where the tenant admin requires multi-factor authentication, you cannot use this flow. Many organizations do that.
- It works only for Work and school accounts (not MSA)
- The flow is available on .net desktop and .net core, but not on UWP

## How to use it?

`PublicClientApplication`contains the method `AcquireTokenByUsernamePasswordAsync`

The following sample presents a simplified case

```CSharp
static async Task GetATokenForGraph()
{
    string authority = "https://login.microsoftonline.com/contoso.com";
    string[] scopes = new string[] { "user.read" };
    PublicClientApplication app = new PublicClientApplication(clientId, authority);
    var accounts = await app.GetAccountsAsync();

    AuthenticationResult result = null;
    if (accounts.Any())
    {
        result = await app.AcquireTokenSilentAsync(scopes, accounts.FirstOrDefault());
    }
    else
    {
        try
        {
            var securePassword = new SecureString();
            foreach (char c in "dummy")        // you should fetch the password
                securePassword.AppendChar(c);  // keystroke by keystroke

            result = await app.AcquireTokenByUsernamePasswordAsync(scopes, "joe@contoso.com",
                                                                   securePassword);
        }
        catch(MsalException)
        {
          // See details below
        }
    }
    Console.WriteLine(result.Account.Username);
}
```

The following sample presents the most current case, with explanations of the kind of exceptions you can get, and their mitigations

```CSharp
static async Task GetATokenForGraph()
{
    string authority = "https://login.microsoftonline.com/contoso.com";
    string[] scopes = new string[] { "user.read" };
    PublicClientApplication app = new PublicClientApplication(clientId, authority);
    var accounts = await app.GetAccountsAsync();

    AuthenticationResult result = null;
    if (accounts.Any())
    {
        result = await app.AcquireTokenSilentAsync(scopes, accounts.FirstOrDefault());
    }
    else
    {
        try
        {
            var securePassword = new SecureString();
            foreach (char c in "dummy")        // you should fetch the password keystroke
                securePassword.AppendChar(c);  // by keystroke

            result = await app.AcquireTokenByUsernamePasswordAsync(scopes, "joe@contoso.com",
                                                                   securePassword);
        }
        catch (MsalUiRequiredException ex) when (ex.Message.Contains("AADSTS65001"))
        {
            // Here are the kind of error messages you could have, and possible mitigations

            // ------------------------------------------------------------------------
            // MsalUiRequiredException: AADSTS65001: The user or administrator has not consented to use the application
            // with ID '{appId}' named '{appName}'. Send an interactive authorization request for this user and resource.

            // Mitigation: you need to get user consent first. This can be done either statically (through the portal), or dynamically (but this
            // requires an interaction with Azure AD, which is not possible with the username/password flow)
            // Statically: in the portal by doing the following in the "API permissions" tab of the application registration:
            // 1. Click "Add a permission" and add all the delegated permissions corresponding to the scopes you want (for instance
            // User.Read and User.ReadBasic.All)
            // 2. Click "Grant/revoke admin consent for <tenant>") and click "yes".
            // Dynamically, if you are not using .NET Core (which does not have any Web UI) by calling (once only) AcquireTokenAsync interactive.
            // remember that Username/password is for public client applications that is desktop/mobile applications.
            // If you are using .NET core or don't want to call AcquireTokenAsync, you might want to:
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
            // Like in the previous case, you might want to use an interactive flow (AcquireTokenAsync()), or Device Code Flow instead.
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
            // MsalServiceException: ADSTS50034: To sign into this application the account must be added to the {domainName} directory.
            // or The user account does not exist in the {domainName} directory. To sign into this application, the account must be added to the directory.
            // The user was not found in the directory
            // Explanation: wrong username
            // Mitigation: ask the user to re-enter the username.
            // ------------------------------------------------------------------------
        }
        catch (MsalServiceException ex) when (ex.ErrorCode == "invalid_request")
        {
            // ------------------------------------------------------------------------
            // AADSTS90010: The grant type is not supported over the /common or /consumers endpoints. Please use the /organizations or tenant-specific endpoint.
            // you used common.
            // Mitigation: as explained in the message from Azure AD, the authority you use in the application needs to be tenanted or otherwise "organizations". change the
            // "Tenant": property in the appsettings.json to be a GUID (tenant Id), or domain name (contoso.com) if such a domain is registered with your tenant
            // or "organizations", if you want this application to sign-in users in any Work and School accounts.
            // ------------------------------------------------------------------------

        }
        catch (MsalServiceException ex) when (ex.ErrorCode == "unauthorized_client")
        {
            // ------------------------------------------------------------------------
            // AADSTS700016: Application with identifier '{clientId}' was not found in the directory '{domain}'.
            // This can happen if the application has not been installed by the administrator of the tenant or consented to by any user in the tenant.
            // You may have sent your authentication request to the wrong tenant
            // Cause: The clientId in the appsettings.json might be wrong
            // Mitigation: check the clientId and the app registration
            // ------------------------------------------------------------------------
        }
        catch (MsalServiceException ex) when (ex.ErrorCode == "invalid_client")
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

        catch (MsalClientException ex) when (ex.ErrorCode == "unknown_user_type")
        {
            // Message = "Unsupported User Type 'Unknown'. Please see https://aka.ms/msal-net-up"
            // The user is not recognized as a managed user, or a federated user. Azure AD was not
            // able to identify the IdP that needs to process the user
            throw new ArgumentException("U/P: Wrong username", ex);
        }
        catch (MsalClientException ex) when (ex.ErrorCode == "user_realm_discovery_failed")
        {
            // The user is not recognized as a managed user, or a federated user. Azure AD was not
            // able to identify the IdP that needs to process the user. That's for instance the case
            // if you use a phone number
            throw new ArgumentException("U/P: Wrong username", ex);
        }
        catch (MsalClientException ex) when (ex.ErrorCode == "unknown_user")
        {
            // the username was probably empty
            // ex.Message = "Could not identify the user logged into the OS. See http://aka.ms/msal-net-iwa for details."
            throw new ArgumentException("U/P: Wrong username", ex);
        }
        catch (MsalClientException ex) when (ex.ErrorCode == "parsing_wstrust_response_failed")
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

See [Azure Active Directory v2.0 and the OAuth 2.0 resource owner password credential](https://review.docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth-ropc) to learn more about the underlying protocol

## Sample illustrating acquiring tokens through Username/Password with MSAL.NET

Sample | Platform | Description
------ | -------- | -----------
[active-directory-dotnetcore-console-up-v2](https://github.com/Azure-Samples/active-directory-dotnetcore-console-up-v2) | Console (.NET Core) | .NET Core console application letting a user signed-in with the Azure AD v2.0 endpoint using username/password to acquire a token for the Microsoft Graph ![topology](https://github.com/Azure-Samples/active-directory-dotnetcore-console-up-v2/blob/master/ReadmeFiles/Topology.png)

> Vanity URL: https://aka.ms/msal-net-up