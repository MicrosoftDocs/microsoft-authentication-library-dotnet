---
title: Multicloud support and instance awareness
---

# Multicloud support and instance awareness

>[!WARNING]
>This feature is only available to 1st party applications (Microsoft applications), which have the same client_id across all clouds. 3rd party applications have different client IDs for each cloud, and cannot use this feature.

## What is instance awareness?

* Instance aware helps complete the scenario where any an account from any cloud can be signed-in using the default value for environment. If instance aware is not activated, the calling app has to provide the correct environment for the account.
* It enables applications to pass in a default public cloud authority to the library and can still get tokens for resources (Graph) from national clouds.
* The user and the resource should belong to single national cloud.
* It is applicable only when using /organizations or /common in the authority url as compared to a tenantId guid.

## What does it mean to enable multi-cloud support in MSAL?

With multi-cloud support enabled, user will have the option to create a `PublicClientApplication` with global authority and if a user enters a username from a national cloud, MSAL will return the token to access resource on the national cloud.

Currently, multi-cloud support is available for `Interactive flows` for web.

## Sample to enable multi-cloud support

```csharp
    IPublicClientApplication pca = PublicClientApplicationBuilder
        .Create(AppId)
        .WithAuthority("https://login.microsoftonline.com/common")
        .WithMultiCloudSupport(true)
        .Build();

    // Acquire a token interactively
    AuthenticationResult result = await pca
        .AcquireTokenInteractive(s_scopes)
        .ExecuteAsync()
        .ConfigureAwait(false);

    // Get Accounts
    var accounts = await pca.GetAccountsAsync().ConfigureAwait(false);

    // Acquire a token silently
    result = await pca
        .AcquireTokenSilent(s_scopes, accounts.FirstOrDefault()) \\ Use the account to make the silent call
        .ExecuteAsync(CancellationToken.None)
        .ConfigureAwait(false);
```

>[!NOTE]
>The environment used to acquire a token can be found using `account.Environment` to create a mapping to respective resource endpoint on the national cloud.
