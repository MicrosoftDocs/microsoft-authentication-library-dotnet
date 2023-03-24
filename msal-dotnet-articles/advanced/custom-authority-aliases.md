---
title: Custom authority aliases
---

# Custom authority aliases

## What is Instance Discovery

Before acquiring tokens, MSAL makes a network call to the Azure AD authority [discovery endpoint](https://login.microsoftonline.com/common/discovery/instance?api-version=1.1&authorization_endpoint=https%3A%2F%2Flogin.microsoftonline.com%2Fcommon%2Foauth2%2Fv2.0%2Fauthorize). The information returned is used to:

- Discover a list of aliases for each cloud (Azure Public, German Cloud, China Cloud etc.). A token issued to an authority in the set is valid for all other authorities in the set.
- Use the preferred_network alias for communication with Azure AD
- Use the preferred_cache alias to store tokens in the cache
- Provide a level of validation for the authority - if a non-existent authority is used, then Azure AD returns an "invalid_instance" [error](https://login.microsoftonline.com/common/discovery/instance?api-version=1.1&authorization_endpoint=https%3A%2F%2Fbogus.microsoftonline.com%2Fcommon%2Foauth2%2Fv2.0%2Fauthorize).

## Instance validation

The validation is important if you obtain your authority dynamically, for example when you call a protect API, it returns a 401 Unauthorized HTTP response which can include a header pointing to an authority that is able to generate a token. If the API is hacked, it could advertise an authority that does not belong to Azure AD and that could steal user credentials.

## Disabling Instance Discovery

MSAL libraries already employ a variety of caching mechanisms for this data. You may still want to bypass the Instance Discovery network call to further optimize performance in some PublicClientApplication scenarios, but you you should only do this if you understand the security risk outlined above. If you provide your own instance metadata, MSAL will always use it and it will never go to the network for this kind of data.

```csharp
var app = PublicClientApplicationBuilder
    .Create(MsalTestConstants.ClientId)
     // or a Guid instead of common
    .WithAuthority(new Uri("https://login.microsoftonline.com/common/"), false) // or a tenanted authority ending in a GUID
    .WithInstanceDicoveryMetadata(instanceMetadataJson) // a json string similar to https://aka.ms/aad-instance-discovery
    .Build();
```

>[!NOTE]
>You have to set the `validateAuthority` flag to `false` because validation is only made against your custom discovery metadata.

### Example instance metadata

Assuming that your authority is `https://login.contoso.net` then a valid instance discovery is shown below. You need to pass this value a string.

```json
{
    "api-version": "1.1",
    "metadata": [
        {
            "preferred_network": "login.contoso.net",
            "preferred_cache": "login.contoso.net",
            "aliases": [
                "login.contoso.net"
            ]
        }
    ]
}
```

## Related MsalError constants

The `MsalError` you can get when using this feature are the following:

| Error | Description |
|:------|:------------|
| `InvalidUserInstanceMetadata ` | You have configured your own custom instance discovery metadata, but the JSON you provided seems to  be invalid. You need a valid `ValidateAuthorityOrCustomMetadata`. Alternatively, it's possible that you have configured your own instance metadata, but have been requesting authority validation. You need to set the validate authority flag to false. |
