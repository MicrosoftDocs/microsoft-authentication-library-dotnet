## AcquireTokenAsync
In MSAL.NET, all the overrides of ``AcquireTokenAsync`` are interactive. 

![image](https://user-images.githubusercontent.com/13203188/37063988-40e52368-2193-11e8-887e-e86b97c54d19.png)

### Parameters
The parameters are the following:
- ``Scopes`` contains an enumeration of strings which define the scopes for which a token is required. If the token is for the Microsoft Graph, the required scopes can be found in api reference of each Microsoft graph API in the section named "Permissions". For instance, to [list the user's contacts](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/user_list_contacts), the scope "User.Read", "Contacts.Read" will need to be used. See also [Microsoft Graph permissions reference](https://developer.microsoft.com/en-us/graph/docs/concepts/permissions_reference). 
- (MSAL.1.x) ``account`` (optional) of type ``IAccount``, provides a hint to the STS about the user for which to get the token. This can be set from the `Account` member of a previous AuthenticationResult, or one of the elements of the collection returned by ``GetAccountsAsync()`` method of the ``PublicClientApplication``. 
- (MSAL.1.x) ``user`` (optional) of type ``IUser``, provides a hint to the STS about the user for which to get the token. This can be set from the User member of a previous AuthenticationResult, or one of the elements of the ``Users`` property of the ``PublicClientApplication``. 
- ``loginHint`` (optional) offers an alternative to user. It's used like ``userIdentifier`` in ADAL. Needs to be passed the `preferred_username` of the IDToken (contrary to ADAL which was requiring the UPN)
- ``UIBehavior`` (optional) is the way to control, in MSAL.NET, the interaction between the user and the STS to enter credentials. It's different depending on the platform (See below)
- ``extraQueryParameters`` (optional) is the same as for ADAL.NET (a string with the following format "key1=value1&key2=value2", enabling developers to pass extra parameters to the STS endpoint. 
- ``extraScopesToConsent`` (optional) enables application developers to specify additional scopes for which users will pre-consent.
               - This can be in order to avoid having them see incremental consent screens when the Web API require them. 
               - This is also indispensable in the case where you want to provide scopes for several resources. See [the paragraph on getting consent for several resources](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-interactively#how-to-get-consent-for-several-resources) below for more details
- ``parent`` (optional) enables, for some platform, to specify the parent UI (window in Windows, Activity in Android, .) which will be the parent of the dialog.

### Controlling the interactivity with the user with the behavior parameter (UIBehavior)
Equivalent to the ``PromptBehavior`` in ADAL.NET, MSAL.NET defines the notion of ``UIBehavior``. This is actually a structure with public static members.

![image](https://user-images.githubusercontent.com/13203188/37064286-39d466e6-2194-11e8-9df9-b5f9ab36567e.png)

The public members are:
- ``Consent``: enables the application developer to have the user be prompted for consent even if consent was granted before. This is done by sending *prompt=consent* to Azure AD
- ``ForceLogin``: enables the application developer to have the user prompted for credentials by the service even if this would not be needed. This can be useful if Acquiring a token fails, to let the user re-sign-in. This is done by sending *prompt=login* to Azure AD
- ``Never`` (for .NET 4.5 and WinRT only) will not prompt the user, but instead will try to use the cookie stored in the hidden embedded web view (See below: Web Views in MSAL.NET). This might fail, and in that case AcquireTokenAsync will throw an exception to notify that a UI interaction is needed, and you will try again by calling an override of AcquireTokenAsync without a ``UIBehavior`` or with a different ``UIBehavior``
- ``SelectAccount``: will force the STS to present the account for which the user has a session. This is useful when applications developers want to let user choose among different identities. This is done by sending ``prompt=select_account`` to Azure AD

### Controlling the location of the dialog with the parent parameters (``UIParent``)
``UIParent`` is used, in the platforms allowing it, to specify the parent window for the authentication dialog. 
- For .NET 4.5 and WinRT, it provides a constructor with an object: the ``OwnerWindow`` parameter
- For Android, it provides a constructor with the parent ``Activity``. [Here's an example for Android.](https://github.com/Azure-Samples/active-directory-xamarin-native-v2/blob/master/UserDetailsClient/UserDetailsClient.Droid/MainActivity.cs#L23)
- For iOS, use the default constructor of `UIParent`. Create the `UIParent` in the `AppDelegate` class. [Here's an example for iOS.](https://github.com/Azure-Samples/active-directory-xamarin-native-v2/blob/master/UserDetailsClient/UserDetailsClient.iOS/AppDelegate.cs#L28)

## More specificities depending on the platforms
Depending on the platforms, you will need to do a bit of extra work to use MSAL.NET. For more details on each platform, see:
- [Xamarin Android specificities](Xamarin-android-specificities)
- [Xamarin iOS specificities](Xamarin-ios-specificities) 
- [UWP specificities](uwp-specificities) 

## How to get consent for several resources

> Note: Getting consent for several resources works for Azure AD v2.0, but not for Azure AD B2C. B2C supports only admin consent, not user consent.

The Azure AD v2.0 endpoint does not allow you to get a token for several resources at once. Therefore the scopes parameter should only contain scopes for a single resource. However, you can ensure that the user pre-consents to several resources by using the `extraScopesToConsent` parameter.

For instance if you have two resources, which have 2 scopes each:
- `https://mytenant.onmicrosoft.com/customerapi` (with 2 scopes `customer.read` and `customer.write`)
- `https://mytenant.onmicrosoft.com/vendorapi` (with 2 scopes `vendor.read` and `vendor.write`)

you should use an [override of AcquireTokenAsync](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.publicclientapplication.acquiretokenasync?view=azure-dotnet#Microsoft_Identity_Client_PublicClientApplication_AcquireTokenAsync_System_Collections_Generic_IEnumerable_System_String__System_String_Microsoft_Identity_Client_UIBehavior_System_String_System_Collections_Generic_IEnumerable_System_String__System_String_Microsoft_Identity_Client_UIParent_) which has the `extraScopesToConsent` parameter

For instance:
```CSharp
string[] scopesForCustomerApi = new string[]
{
  "https://mytenant.onmicrosoft.com/customerapi/customer.read",
  "https://mytenant.onmicrosoft.com/customerapi/customer.write"
};
string[] scopesForVendorApi = new string[] 
{
 "https://mytenant.onmicrosoft.com/vendorapi/vendor.read", 
 "https://mytenant.onmicrosoft.com/vendorapi/vendor.write" 
};

var accounts = await app.GetAccountsAsync(); // for MSAL.NET 2.x or otherwise app.Users in MSAL.NET 1.x
var result = await app.AcquireTokenAsync(scopesForCustomerApi, 
                                         accounts.FirstOrDefault(), 
                                         uiBehavior, 
                                         string.Empty, 
                                         scopesForVendorApi,
                                         app.Authority,
                                         uiParent); 
```

This will get you an access token for the first Web API.
Then when you need to call the second one, you can call

```CSharp
AcquireTokenSilentAsync(scopesForVendorApi);
```

See [this](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/550#issuecomment-383572227) GitHub issue for more context.

## Samples illustrating acquiring tokens interactively with MSAL.NET
Sample | Platform | Description 
------ | -------- | -----------
[active-directory-dotnet-desktop-msgraph-v2](http://github.com/azure-samples/active-directory-dotnet-desktop-msgraph-v2) | Desktop (WPF) | Windows Desktop .NET (WPF) application calling the Microsoft Graph API. ![](https://github.com/Azure-Samples/active-directory-dotnet-desktop-msgraph-v2/blob/master/ReadmeFiles/Topology.png)
[active-directory-dotnet-native-uwp-v2](https://github.com/azure-samples/active-directory-dotnet-native-uwp-v2) | UWP | A Windows Universal Platform client application using msal.net, accessing the Microsoft Graph for a user authenticating with Azure AD v2.0 endpoint. ![](https://github.com/Azure-Samples/active-directory-dotnet-native-uwp-v2/blob/master/ReadmeFiles/Topology.png)
[https://github.com/Azure-Samples/active-directory-xamarin-native-v2](https://github.com/Azure-Samples/active-directory-xamarin-native-v2) | Xamarin iOS, Android, UWP | A simple Xamarin Forms app showcasing how to use MSAL to authenticate MSA and Azure AD via the AADD v2.0 endpoint, and access the Microsoft Graph with the resulting token. ![](https://github.com/Azure-Samples/active-directory-xamarin-native-v2/blob/master/ReadmeFiles/Topology.png)
[https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2) | WPF, ASP.NET Core 2.0 Web API | A WPF application calling an ASP.NET Core Web API using Azure AD v2.0. ![](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/blob/master/ReadmeFiles/topology.png)
