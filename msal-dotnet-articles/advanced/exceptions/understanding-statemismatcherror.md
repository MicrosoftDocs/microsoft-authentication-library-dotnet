---
title: Understanding StateMismatchError
dscription: "Learn about StateMismatchError in MSAL.NET, its properties, and how to handle it effectively."
author: Dickson-Mwendia
manager: 
ms.author: dmwendia
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: conceptual
ms.custom: 
#Customer intent: 
---

# Understanding `StateMismatchError`

MSAL verifies the state returned by the server with the original state as a security protocol. In case the state is different this exception is thrown.

## Known issues

For apps when using a long Facebook Id observed to be 33 characters or more for example somelongemailaddressfortest@gmail.com, this exception is thrown. Embedded web view in desktop apps uses Internet Explorer and it truncates the URL to 2083 characters which causes the value of state parameter in the URL to be truncated. This causes the returned state to be different from the original state.

To mitigate please use `.WithUseEmbeddedWebView(false)` and refer to [Using web browsers (MSAL.NET)](/azure/active-directory/develop/msal-net-web-browsers).

## References

- [Maximum URL length is 2,083 characters in Internet Explorer](https://support.microsoft.com/help/208427/maximum-url-length-is-2-083-characters-in-internet-explorer)
