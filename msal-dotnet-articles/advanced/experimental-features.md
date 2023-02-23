# Experimental features in MSAL.NET

## API promise

MSAL is strict about semantic versioning and will not introduce breaking changes without incrementing the major version.

## Experimental APIs

Some of the new APIs exposed by MSALs are marked as `Experimental`. These APIs may change without fulfilling the promise above. As such, it is not recommended to use these APIs in production, but you are encouraged to try them out, provide feedback etc.

Starting with MSAL 4.8, developers need to add a flag to be able to use experimental features, otherwise an exception will be thrown. 

```csharp
 var pca = PublicClientApplicationBuilder
                .Create(clientId)
                .WithExperimentalFeatues()
                .Build();
```
