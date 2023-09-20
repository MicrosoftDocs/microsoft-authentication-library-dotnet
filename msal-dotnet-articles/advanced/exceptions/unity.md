---
title: Troubleshooting MSAL.NET in Unity applications
description: Learn how to troubleshoot MSAL.NET in Unity applications. Understand the cause of runtime exceptions and discover effective solutions.
---

# Troubleshooting MSAL.NET in Unity applications

MSAL 4.48.0 and above stopped using reflection on its `net6` target. This is the only path forward with Unity.

## Member not found at runtime

### The issue

When using MSAL.NET in a Unity UWP app, the application builds successfully. However at runtime, exceptions, like the ones below, are thrown that some members don't exist in MSAL.NET's code:

```bash
Error on deserializing read-only members in the class: No set method for property 'Claims' in type 'Microsoft.Identity.Client.OAuth2.OAuth2ResponseBase'.
  at System.Runtime.Serialization.DataContract+DataContractCriticalHelper.ThrowInvalidDataContractException
   (System.String message, System.Type type) [0x00000] in <00000000000000000000000000000000>:0 
  at System.Runtime.Serialization.DataContract.ThrowInvalidDataContractException
   (System.String message, System.Type type) [0x00000] in <00000000000000000000000000000000>:0 
```

```bash
Error setting value to 'TenantDiscoveryEndpoint' on 'Microsoft.Identity.Client.Instance.Discovery.InstanceDiscoveryResponse'.
 at Microsoft.Identity.Json.Serialization.ExpressionValueProvider.SetValue
   (System.Object target, System.Object value) [0x00000] in <00000000000000000000000000000000>:0 \r\n
 at Microsoft.Identity.Json.Serialization.JsonSerializerInternalReader.SetPropertyValue
   (Microsoft.Identity.Json.Serialization.JsonProperty property, Microsoft.Identity.Json.JsonConverter propertyConverter,
     Microsoft.Identity.Json.Serialization.JsonContainerContract containerContract, Microsoft.Identity.Json.Serialization.JsonProperty containerProperty,
     Microsoft.Identity.Json.JsonReader reader, System.Object target) [0x00000] in <00000000000000000000000000000000>:0
```

### Cause and solution

The issue comes from Unity IL2CPP plugin. When optimizing code (using code stripping), it removes needed dependencies for reflection to work (because it can't properly detect that usage). The MSAL.NET team investigated removing reflection related code from MSAL but it proved to be very impractical. Unity themselves have this documented in their docs ([Managed code stripping](https://docs.unity3d.com/Manual/ManagedCodeStripping.html#LinkXML)) and recommend to use Link XML method as one of the solutions to this issue. This is our recommendation as well.

Add below entries into the root `Assets/link.xml` folder:

```xml
<linker>
 <assembly fullname="Microsoft.Identity.Client" preserve="all" />
 <assembly fullname="System" preserve="all" />
 <assembly fullname="System.Core" preserve="all" />
</linker>
```

### See also

[#1185](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1185), [#2231](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/2231)
