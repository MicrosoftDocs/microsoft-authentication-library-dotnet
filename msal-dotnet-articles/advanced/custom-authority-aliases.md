Note: Feature available from: 4.2

## What is Instance Discovery 

Before acquiring tokens, MSAL makes a network call to the AAD authority [discovery endpoint](https://login.microsoftonline.com/common/discovery/instance?api-version=1.1&authorization_endpoint=https%3A%2F%2Flogin.microsoftonline.com%2Fcommon%2Foauth2%2Fv2.0%2Fauthorize). The information returned is used to:

- discover a list of aliases for each cloud (Azure Public, German Cloud, China Cloud etc.). A token issued to an authority in the set is valid for all other authorities in the set. 
- use the preferred_network alias for communication with AAD
- use the preferred_cache alias to store tokens in the cache
- provide a level of validation for the authority - if a non-existent authority is used, then AAD returns an "invalid_instance" [error](https://login.microsoftonline.com/common/discovery/instance?api-version=1.1&authorization_endpoint=https%3A%2F%2Fbogus.microsoftonline.com%2Fcommon%2Foauth2%2Fv2.0%2Fauthorize) 

#### Instance Validation 

The validation is important if you obtain your authority dynamically, for example when you call a protect API, it returns a 401 Unauthorized HTTP response which can include a header pointing to an authority that is able to generate a token. If the API is hacked, it could advertise an authority that does not belong to AAD and that could steal user credentials. 

## Disabling Instance Discovery

MSAL libraries already employ a variety of caching mechanisms for this data. You may still want to bypass the Instance Discovery network call to further optimize performance in some PublicClientApplication scearios, but you you should only do this if you understand the security risk outlined above. If you provide your own instance metadata, MSAL will always use it and it will never go to the network for this kind of data. 

```csharp

var app = PublicClientApplicationBuilder
    .Create(MsalTestConstants.ClientId)
     // or a Guid instead of common
    .WithAuthority(new Uri("https://login.microsoftonline.com/common/"), false) // or a tenanted authority ending in a GUID
    .WithInstanceDicoveryMetadata(instanceMetadataJson) // a json string similar to https://aka.ms/aad-instance-discovery
    .Build();
```
Note: You have to set the validateAuthority flag to false because validation is only made against your custom discovery metadata.

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

The MsalError you can get when using this feature are the following:

Error | Description
----  | ----------
`InvalidUserInstanceMetadata ` | You have configured your [own custom instance discovery metadata](https://aka.ms/msal-net-custom-instance-metadata), but the json you provided seems to  be invalid. See https://aka.ms/msal-net-custom-instance-metadata for an example of a valid
`ValidateAuthorityOrCustomMetadata` | You have configured your own instance metadata, but have been requesting authority validation. You need to set the validate authority flag to false. See https://aka.ms/msal-net-custom-instance-metadata for more details.
