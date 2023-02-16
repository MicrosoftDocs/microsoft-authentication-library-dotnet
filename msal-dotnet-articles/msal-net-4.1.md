# MSAL.NET 4.1 released

We are excited to announce an incremental update of MSAL.NET addressing some of the issues you raised, and bringing:

- [Improved user experience with the system browser on .NET core](#improved-experience-with-the-system-web-browser-on-net-core): MSAL brings new options to control the way the System Web browser will handle the communication with the user in case of success or failure
- [Confidential client applications now support client assertions](#confidential-client-applications-now-support-client-assertions)
- [GetAccounts and AcquireTokenSilent are now less network chatty](#getaccounts-and-acquiretokensilent-are-now-less-network-chatty)
- [Better control over correlation IDs](#better-control-over-correlation-ids)
- [Bug fixes](#bug-fixes)

## Improved experience with the system web browser (on .NET Core)

From MSAL.NET 4.0.0, you have been able to use the interactive token acquisition with .NET Core, by delegating the sign-in and consent part to the system Web browser on your machine. MSAL.NET 4.1, brings improvements to this experience by helping you run a specific browser if you wish, and by giving you ways to decide what to display to the user in case of a successful authentication, and in case of failure

MSAL.NET 4.1 adds a new class named `SystemWebViewOptions` which enables you to specify:
- the URI to navigate to (`BrowserRedirectError`), or the HTML fragment to display (`HtmlMessageError`) in case of sign-in / consent errors in the System web browser
- the URI to navivate to (`BrowserRedirectSuccess`), or the HTML fragment to display (`HtmlMessageSuccess`) in case of successful sign-in / consent.
- the action to run to start the system browser. For this you cnan provide your own implementation by setting the `OpenBrowserAsync` delegate. The class also provides a default implementation for two browsers: `OpenWithEdgeBrowserAsync` and `OpenWithChromeEdgeBrowserAsync`, respectively for Microsoft Edge and [Edge on Chromium](https://www.windowscentral.com/faq-edge-chromium).

```CSharp
public class SystemWebViewOptions
{
 public SystemWebViewOptions();
 public Uri BrowserRedirectError { get; set; }
 public Uri BrowserRedirectSuccess { get; set; }
 public string HtmlMessageError { get; set; }
 public string HtmlMessageSuccess { get; set; }
 public Func<Uri, Task> OpenBrowserAsync { get; set; }
 public static Task OpenWithChromeEdgeBrowserAsync(Uri uri);
 public static Task OpenWithEdgeBrowserAsync(Uri uri);
}
```

To use this structure you can write something like the following:

```CSharp
IPublicClientApplication app;
...

options = new SystemWebViewOptions
{
 HtmlMessageError = "<b>Sign-in failed. You can close this tab ...</b>",
 BrowserRedirectSuccess = "https://contoso.com/help-for-my-awesome-commandline-tool.html"
};

var result = app.AcquireTokenInteractive(scopes)
                .WithEmbeddedWebView(false)       // The default in .NET Core
                .WithSystemWebViewOptions(options)
                .Build();
```

Error handling

The MsalError class was augmented with the following errors related to the System web browser configuration

```CSharp
public static class MsalError 
{
 ...
 public const string SystemWebviewOptionsNotApplicable = "embedded_webview_not_compatible_default_browser";
 public const string WebviewUnavailable = "no_system_webview";
}
```

## Confidential client applications now support client assertions

[Content now available in the obo section of the wiki](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/on-behalf-of#client-assertions)

### New capabilities for confidential client applications to prove their identity

In order to prove their identity, confidential client applications exchange a secret with Azure AD. This can be a:
- a client secret (application password), 
- a certificate, which is really used to build a signed assertion containing standard claims. 
This can also be a signed assertion directly.

MSAL.NET 4.1 adds a new capabilities for this advanced scenario: in addition to `.WithClientSecret()` and `.WithCertificate()`, it now provides two new methods: `.WithClientAssertion()` and `.WithClientClaims()`.

### WithClientAssertion

The first method takes a signed client assertion. It's expected to be a Base64 encoding of a JWT token which needs to contain mandatory claims. To use it:

```CSharp
string signedClientAssertion = ComputeAssertion();
app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                                          .WithClientAssertion(signedClientAssertion)
                                          .Build();
```

The claims expected by Azure AD are:

Claim type | Value | Description
---------- | ---------- | ----------
aud | https://login.microsoftonline.com/{tenantId}/v2.0 | The "aud" (audience) claim identifies the recipients that the JWT is intended for (here Azure AD) See [RFC 7519, Section 4.1.3]
exp | Thu Jun 27 2019 15:04:17 GMT+0200 (Romance Daylight Time) | The "exp" (expiration time) claim identifies the expiration time on or after which the JWT MUST NOT be accepted for processing. See [RFC 7519, Section 4.1.4]
iss | {ClientID} | The "iss" (issuer) claim identifies the principal that issued the JWT. The processing of this claim is generally application specific. The "iss" value is a case-sensitive string containing a StringOrURI value. [RFC 7519, Section 4.1.1]
jti | (a Guid) | The "jti" (JWT ID) claim provides a unique identifier for the JWT. The identifier value MUST be assigned in a manner that ensures that there is a negligible probability that the same value will be accidentally assigned to a different data object; if the application uses multiple issuers, collisions MUST be prevented among values produced by different issuers as well. The "jti" claim can be used to prevent the JWT from being replayed. The "jti" value is a case-sensitive string. [RFC 7519, Section 4.1.7]
nbf | Thu Jun 27 2019 14:54:17 GMT+0200 (Romance Daylight Time) | The "nbf" (not before) claim identifies the time before which the JWT MUST NOT be accepted for processing. [RFC 7519, Section 4.1.5]
sub | {ClientID} | The "sub" (subject) claim identifies the subject of the JWT. The claims in a JWT are normally statements about the subject. The subject value MUST either be scoped to be locally unique in the context of the issuer or be globally unique. The See [RFC 7519, Section 4.1.2]


### WithClientClaims

WithClientClaims(X509Certificate2 certificate, IDictionary<string, string> claimsToSign, bool mergeWithDefaultClaims = true) by default will produce a signed assertion containing the claims expected by Azure AD plus additional client claims that you want to send. Here is a code snippet on how to do that.

```CSharp
string ipAddress = "192.168.1.2";
X509Certificate2 certificate = ReadCertificate(config.CertificateName);
app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                                          .WithAuthority(new Uri(config.Authority))
                                          .WithClientClaims(certificate, 
                                                                      new Dictionary<string, string> { { "client_ip", ipAddress } })
                                          .Build();

```

If one of the claims in the dictionary that you pass in is the same as one of the mandatory claims, the additional claims's value will be taken into account (it will override the claims computed by MSAL.NET)

If you want to provide your own claims, including the mandatory claims expected by Azure AD, simply pass in a false for the mergeWithDefaultClaims parameter.

### Error handling


The MsalError class was augmented with the following error related to client credentials

```CSharp
public static class MsalError 
{
 ...
 public const string ClientCredentialAuthenticationTypesAreMutuallyExclusive = "Client_Credential_Authentication_Types_Are_Mutually_Exclusive";
}
```

This error will be thrown if you attempt to use several client credentials (certificate, client secret, Signed claims, signed assertion)

## Better control over Correlation IDs

Correlation IDs are used in logging, and help troubleshooting issues. When a transaction is starting at the front door of a service (Gateway), it gets a correlation ID, this ID is preserved through the calls to MSAL, Azure AD, down inside it sub components, and back. Each of the components *knows* the correlation ID coming into it, preserves it in their logs and then passes it on when it delegates to other components.

MSAL.NET was so far generating a correlation ID. But if the application developer wanted to create their own Correlation ID, and pass it to MSAL.NET, this was not yet possible. This is, in particular, imlportant in Web APIs calling downstream APIS. Then we did not surface the Correlation ID other than in the logs, so if the application wanted to access it it had basically to parse a log line.

Starting from MSAL.NET 4.1:
- AuthenticationResult now has a property named `CorrelationId`. It's a `Guid` returning the correlation ID used for the authentication request. This fullfil the need for apps developer to know the correlation ID.
  ```CSharp
  AuthenticationResult result = await ....
  Guid usedCorrelationId = result.CorrelationId
  ```

- There is now an API (`WithCorrelationId(Guid)`), applicable on any AcquireTokenXXX(), to set the correlation ID. This fullfils the need for service developers who themselve receive a CorrelationID and want the token acquisition to be part of the same transaction.
  ```CSharp
  result = await AcquireTokenXX(scopes)
                 .WithCorrelationId(correlationId)
                 .ExecuteAsync();

  ```

## GetAccounts and AcquireTokenSilent are now less network chatty

**MSAL.NET will make network calls less often when developers invoke`GetAccountsAsync` and `AcquireTokenSilent`**.

Some of you asked us to support disconnected scenarios; when the user had previously signed-in on a device, you wanted your app to get the available account(s) without the device having to be connected to the network. This was not the case until MSAL.NET 4.1. Indeed, Azure AD provides an instance discovery endpoint which lists environment aliases for each cloud. In order to optimize SSO, MSAL fetches this list and caches it when the application start. As such, MSAL had to make a network call even in simple cases like `GetAccountsAsync`. This improvement bypasses the need for this network call if the environments used are the standard ones. This work is tracked by [MSAL issue 1174](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1174)

The list of aliases can be seen [here](https://login.microsoftonline.com/common/discovery/instance?api-version=1.1&authorization_endpoint=https%3A%2F%2Flogin.microsoftonline.com%2Fcommon%2Foauth2%2Fv2.0%2Fauthorize). As long as an application uses an authority with one of these aliases, `GetAccountsAsync` and `AcquireTokenSilent` when a valid access token exists - will no longer need to make a network call. In these cases MSAL.NET supports the **disconnected mode**

The MSAL team will continue to work on minimizing network access and work will be tracked with the issue [MSAL issue 1174](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1174)


## Bug fixes
- **When using the `ConfidentialClientApplicationOptions` and including, for example `Instance = "https://login.microsoftonline.com/"`, MSAL.NET was concatenating the double-slash**. MSAL.NET will now check for a trailing slash and remove it. There is no action needed on the part of the developer. See [#1196](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1196) for details.
- client credentials with certificates were broken for application using common. See [#891](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/891). This is now fixed
- **When using ADFS 2019, if no login-hint was included in the call, a null ref was thrown**. See [#1214](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1214) for details.
- **On iOS, for certain older auth libraries, sharing the cache with MSAL.NET, there was an issue with null handling in json**. The json serializer in MSAL.NET no longer writes values to json for which the values are null, this is especially important for foci_id. See [#1189](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1189) and [#1176](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1176) for details.
- **When using `.WithCertificate()` and `/common/` as the authority in a confidential client flow, MSAL.NET was creating the `aud` claim of the client assertion as `"https://login.microsoftonline.com/{tenantid}/v2.0"`**. Now, MSAL.NET will honor both a tenant specific authority and common or organizations when creating the `aud` claim. [#891](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/891)