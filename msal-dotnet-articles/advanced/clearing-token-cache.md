# Clearning the token cache

Clearing the cache is achieved by removing the accounts from the cache.

This does not remove the session cookie which is in the browser, though.

The code is the following where app is a `IClientApplicationBase`

```CSharp
   // clear the cache
   var accounts = await app.GetAccountsAsync();
   while (accounts.Any())
   {
    await app.RemoveAsync(accounts.First());
    accounts = await app.GetAccountsAsync();
   }
```