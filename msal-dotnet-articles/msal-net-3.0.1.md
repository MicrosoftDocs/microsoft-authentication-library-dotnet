# Changes in MSAL.NET 3.0.1

We are moving forward towards our goal of shipping a very usable and clean API in MSAL.NET v3. This release of MSAL.NET 3.0.1 brings:
- small breaking changes vs MSAL 3.0.0, which should not impact you. We are still deciding on what the right API should be, and there are a few iterations there.
- new additions to the API
- MSAL.NET for .NET Core moved to .NET Core 2.1.

Note: This time, we did not strictly respect the semantic versioning because it's still a -preview NuGet package (and we thought it would be odd to move from MSAL 3.0.0-preview to MSAL 4.0.0-preview because we had a breaking change in a preview version. your feedback is welcome here)

## Breaking changes from 3.0.0

The V3.0 `AcquireTokenSilent` API could be called with only the scope. If the token cache contained only one identity, you would get it, otherwise it would throw.

Now, AcquireTokenSilent has two overrides that require you to pass-in the account or the `loginHint`.
`AcquireTokenSilent(IEnumerable<string> scopes, IAccount account=null)`
`AcquireTokenSilentParameterBuilder AcquireTokenSilent(IEnumerable<string> scopes, string loginHint)`

`MsalServiceException` used to contain the `SubError` property in MSAL 3.0.0, which we had not documented yet, BTW. As we have not yet decided how to support the error details across all the MSAL libraries, the .NET team have decided to remove it until things have converged, instead of providing a property which might change in the future.

`ITokenCache`'s `DeserializeXX` methods used to contain a `merge` Boolean in MSAL 3.0.0, which did not do anything. We removed it from the APIs as the methods always merge the result of the deserialization into the token cache

`WithClaims` could be passed at app creation, which did not make sense (we overlooked that). It's now only passed on the AcquireToken methods (which we should have done from the start)

`ICustomWebUi.AcquireAuthorizationCodeAsync` now takes a cancellation Token.

## Additions to the API

`MsalError` contains all the error messages

`MsalException` and its derived exception can now be serialized to JSon and deserialized

At both the app creation and the token acquisition, you can now pass extra query parameters as a string (in addition to a Dictionary<string,string> introduced in MSAL 3.0.0

MSAL.NET symbols are now published on the Microsoft Symbol server, which is useful when you are debugging.

## Bug fixes

We fixed a number of issues. For details see [3.0.1](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/milestone/21)
