---
title: Retry policies
description: Learn how to implement a custom retry policy for token acquisition operations in .NET with MSAL. Increase your service availability with our detailed guide.
ms.service: msal
ms.subservice: msal-dotnet
---

# Retry Policies are baked in the libary

MSAL has its own retry policies. In rare cases you can choose to disable the internal retry policies and add your own. See [HttpClient tips](../httpclient.md).

### MSAL implements a simple "retry-once" for errors with HTTP error codes 5xx

MSAL.NET implements a simple retry-once with 1 second delay mechanism for errors with HTTP error codes 500-600, for the token endpoint.
For managed identity, the retry follows the guidelines of each source.

## Customize the HTTP stack

In some cases, such as using proxies, you might want to customize the Http Stack. See [HttpClient tips](../httpclient.md) for details.


