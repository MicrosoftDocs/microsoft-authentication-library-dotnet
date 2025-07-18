---
title: Using MSAL.NET with Android and iOS emulators
description: "How to use MSAL.NET with Android and iOS device emulators."
author: Dickson-Mwendia
manager: 
ms.author: dmwendia
ms.date: 05/20/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: concept-article
ms.custom: 
#Customer intent: 
---

# Using MSAL.NET with Android and iOS emulators

>[!NOTE]
>The MSAL .NET team recommends testing with an Android or iOS device whenever possible, as there are subtle differences between authentication with an emulator and a device.

Some of those issues are documented here in the [native Android MSAL library Wiki](https://github.com/AzureAD/microsoft-authentication-library-for-android/wiki/Android-Emulator-with-MSAL).

For iOS, there are differences between SSO and accessing keychain when using an emulator or a device. Before opening an issue or reporting a bug, please see if the issue replicates on a device.
