---
title: Default reply URI
---

# Default reply URI

In MSAL.NET 4.1+ The default redirect URI(Reply URI) can now be set with the `public PublicClientApplicationBuilder WithDefaultRedirectUri()` method. This method will set the public client applications redirect uri property to the default recommended redirect uri for public client applications.

This method's behavior is dependent upon the platform that you are using at the time. Here is a table that describes what redirect uri is set on certain platforms:

Platform  | Redirect URI  
---------  | --------------
desktop app (.NET FW) | `https://login.microsoftonline.com/common/oauth2/nativeclient`
UWP | value of `WebAuthenticationBroker.GetCurrentApplicationCallbackUri()`
.NET Core | `http://localhost`

For the UWP platform, we want to enhance the experience by enabling SSO with the browser by setting the value to the result of WebAuthenticationBroker.GetCurrentApplicationCallbackUri(). 

For .NET Core, we are setting the value to the local host to enable the user to use the system browser for interactive authentication since .NET Core does not have a UI for the embedded web view at the moment.

> [!NOTE]
> For embedded browsers in desktop scenarios the redirect uri used is intercepted by MSAL to detect that a response is returned from the identity provider that an auth code has been returned. This uri can therefor be used in any cloud without seeing an actual redirect to that uri. This means you can and should use `https://login.microsoftonline.com/common/oauth2/nativeclient` in any cloud. If you prefer you can also translate this to another uri as long as you configure the redirect uri correctly with MSAL. Specifying the above in the application registration means there is the least amount of setup in MSAL.