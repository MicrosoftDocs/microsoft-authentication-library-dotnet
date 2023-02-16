> This page is about MSAL.NET 2.x. For client applications in MSAL.NET 1.x, see [Clients applications in MSAL.NET 1.x](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Client-Applications-in-MSAL-1.x). For a summary of the differences see https://aka.ms/msal-net-msal-2-released
>
> For client applications in MSAL.NET 3.x, see [Clients applications](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Client-Applications). For a summary of the differences see [MSAL.NET-3-released](MSAL.NET-3-released)

## Public Client and Confidential Client applications 

Contrary to ADAL.NET (which proposes the notion of [AuthenticationContext](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/AuthenticationContext:-the-connection-to-Azure-AD), which is a connection to Azure AD), MSAL.NET, proposes a clean separation between public client applications, and [confidential client applications](https://tools.ietf.org/html/rfc6749):
- **Confidential client applications** are typically applications which run on servers (Web Apps, Web API, or even service/daemon applications). They are considered difficult to access, and therefore capable of keeping an application secret. Confidential clients are able to hold configuration time secrets. Each instance of the client has a distinct configuration (including clientId and secret). These values are difficult for end users to extract. A web app is the most common confidential client. The clientId is exposed through the web browser, but the secret is passed only in the back channel and never directly exposed. 
- On the contrary **public client applications** are typically applications which run on devices (phones for instance) or desktop machines. They are not trusted to safely keep application secrets, and therefore access Web APIs in the name of the user only (they only support public client flows). Public clients are unable to hold configuration time secrets, and as a result have no client secret

MSAL.NET defines ``IPublicClientApplication`` interfaces for public client and ``IConfidentialClientApplication`` for confidential client applications, as well as a base interface ``IClientApplicationBase`` for the contract common to both types of applications.

![image](https://user-images.githubusercontent.com/13203188/44658598-b26d5900-aa01-11e8-9c91-e40972c27e30.png)

### High level commonalities and differences

It's immediately obvious that:
- Both kinds of applications can acquire a token silently (in case the token is already in the token cache)
- Public client applications have many ways of acquiring a token. Most involve a UI, which can be controlled by the application developer. But there are three special public client application flows, which don't present a UI in a web control 
  - Integrated Windows Authentication, for apps running on windows domain joined or AAD joined machines, 
  - Device Code Flow, which is for devices which are not capable of displaying a web page but are limited to displaying text
  - Username / password flow (where no UI is presented) and therefore have several restrictions, especially related to conditional access not being supported.
    
- Confidential client applications, on the other hand, don't have methods to acquire tokens interactively. Instead, they support the following grants:
	- Client credential, which is used when the service wants to call another service with no user - here it's important to understand that the client is the application. It's not a user
	- On behalf of flow, which is used when services want to call another service in the name of a user, which credentials were passed in some way, usually a token.
- The application also manages users. It's possible to get a user from its identifier, and to remove a user (of course user management does not make sense in the client credential case)

Contrary to ADAL.NET's Authentication context, the ``clientID`` (also named *applicationID* or *appId*) is passed once at the construction of the Application, and no longer needs to be repeated when acquiring a token. This is the case both for a public and a confidential client application. Constructors of confidential client applications are also passed client credentials.

### PublicClientApplication

![image](https://user-images.githubusercontent.com/13203188/44658761-2ad41a00-aa02-11e8-9f6e-d8e8d5f0baaf.png)

the application developer can also provide
- the authority and its validation
- a token cache that s/he would have de-serialized.
- the redirect Uri

#### Authority

##### Default authority
If you don't specify the authority in the construction of the application, the default value for authority is the value of the ``DefaultAuthority`` static member that is "https://login.microsoftonline.com/common/". This means your applications multi-tenant by default. Note that this needs to be consistent with the application registration as the new application registration experience ([App Registration Preview](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredAppsPreview)) lets you specify the audience (Supported account types) for the application. See [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app) for more details about it.

##### Tenanted authority

It's always possible for the application developer to target a specific tenant for a line of business application by using $"https://login.microsoftonline.com/{tenant}/" where `tenant` is either the tenant ID for the Azure Active Directory, or a domain associated with the azure active directory

##### Other well known authorities

It's also possible to target a more specific category of users (such as only business or only personal accounts). For this, the authority will need to be
- https://login.microsoftonline.com/organizations/ for businesses (read Azure AD accounts). Interestingly using /organizations in MSAL targets the same accounts than using /common with ADAL (as the Azure AD v1.0 endpoint is only about Azure AD accounts)
- https://login.microsoftonline.com/consumers/ for personal accounts (read MSA)
- Finally, to apply an Azure AD B2C policy, you can use $"https://login.microsoftonline.com/tfp/{tenant}/{policyName}" where `tenant` is the name of the Azure AD B2C tenant, and policy name the name of the policy (for instance "b2c_1_susi" for sign-in/sing-up)

##### Authority validation

You have the possibility of bypassing the authority validation by setting the ``ValidateAuthority`` property of the application to ``false``. Note that the validation is not done in the constructor, but the first time an interaction with the STS is required (for instance the first time the developer tries to acquire a token).
You will have to do it for Azure AD B2C tenant for the moment.

#### Redirect URI

For public client applications developers using MSAL.NET:
- don't need to pass the ``RedirectUri`` as its automatically computed by MSAL.NET. This RedirectUri is set to the following values depending on the platform:

- ``urn:ietf:wg:oauth:2.0:oob`` for all the Windows platforms
- ``msal{ClientId}://auth`` for Xamarin Android and iOS

However, the redirect URI needs to be configured in the [App Registration Preview](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredAppsPreview)

![image](https://user-images.githubusercontent.com/13203188/48255596-801ea580-e40d-11e8-8c26-cf167388d5c4.png)

it is possible to override the Redirect Uri using the ``PublicClientApplication`` ``RedirectUri`` property for instance:

- ``RedirectUriOnAndroid`` = "msauth-5a434691-ccb2-4fd1-b97b-b64bcfbc03fc://com.microsoft.identity.client.sample";
- ``RedirectUriOnIos`` = "adaliosxformsapp://com.yourcompany.xformsapp";

If you do, you'll also need to add your custom RedirectUri to the application registration

#### UseCorporateNetwork

Finally, MSAL ``PublicClientApplication`` for WinRT have another boolean property named ``UseCorporateNetwork``. Its behavior is explained in detail in [Properties of PlatformParameter specific to WinRT and UWP](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/uwp-specificities#properties-of-platformparameter-specific-to-winrt-and-uwp-corporate-network).


###  ConfidentialClientApplication

![image](https://user-images.githubusercontent.com/13203188/44658812-5eaf3f80-aa02-11e8-9f4a-0f815d4af947.png)

In addition to the ``ClientId``, confidential client application constructors also take:
- A redirect URI (or Reply URI), which is the URI at which Azure AD will contact back the application with the token. This can be the URL of the Web application / Web API if the confidential is one of these. 
**Tip:** This redirect URI needs to be registered in the app registration. This is especially important when you deploy an application that you have initially tested locally; you then need to add the reply URL of the deployed application in the application registration portal.
- client credentials of type ClientCredential which represent the application identity (a secret string, or a certificate which public key was shared with Azure AD part of the application registration)

![image](https://user-images.githubusercontent.com/13203188/37058029-1dff9ffc-2181-11e8-936a-490952144aea.png)

Optionally, the application developer can also provide
- the authority, if he does not want to target the Azure AD common endpoint (which is ``DefaultAuthority``)
- two caches of type TokenCache:
  - the user token cache (for the on-behalf-of grant). This is the same cache as in the case of public client applications.
  - the application cache, which is only used for the client credential flow where the application acquires tokens in its own name, that is for no user (``AcquireTokenForClientAsync``)

> Important: don't use the same cache instance for both arguments!