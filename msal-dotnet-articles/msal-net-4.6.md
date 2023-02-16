We are excited to announce the release of MSAL.NET 4.6.0, which brings improvements to iOS 13 with broker support, to token cache serialization, as well as several bug fixes, in particular around specification of the authority.

### New Features

**On iOS 13, users of your app leveraging  the broker (Microsoft Authenticator) will be prompted less**
We've improved the interaction with the iOS broker. When MSAL .NET interacts with the iOS broker, starting with version 6.3.19 of the Authenticator app, MSAL .NET will receive a special application token from the broker, which it stores in the keychain. Subsequent calls to the broker will include this application token. When presented with the correct application token, the broker may show fewer prompts to the user. 

**New TokenCacheNotificationArgs.IsApplicationCache property simplifies development of token cache serialization**
TokenCacheNotificationArgs now includes a flag named `IsApplicationCache`, which will disambiguate between the app token cache and the user token cache. This will help developers writing more targeted cache storage code. The MSAL extension libraries, such as Microsoft.Identity.Web (not currently published to NuGet) will immediately benefit from it. 

### Bug Fixes

**Device Code Flow now provides an explicit error message if you forgot to enable public client flows during app registration**
Device Code Flow would fail with a misleading error message if the app was misconfigured in the Azure Application Portal. Indeed, you must enable "public client flows", when using Device Code Flow, Integrated Windows Auth and Username/Password for these flows to work #1407

**Performance improvements. Setting a non tenanted authority when calling AcquireTokenXX is now ignored**
Setting a `common` authority override when calling AcquireTokenXX used to always cause a cache failure. Indeed, MSAL allows you to override the authority by specifying it on the `AcquireTokenSilent` builder, with the goal of allowing users to specify a tenant ID (to get a token for the same user, in a different tenant. Think for instance of iterating through your Azure subscriptions). #1456 

**Usability improvements. It's possible to specify `.WithAuthority(audience)` and `.WithTenantId()**`
Setting an authority audience of `AzureADMyOrg` and a tenant ID would fail. #1320 
 
### Fundamentals

Add tests to check cache format interoperability between MSAL Java and MSAL .NET