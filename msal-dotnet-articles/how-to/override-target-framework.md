---
title: Overriding target framework
description: "In rare circumstances you might want to override NuGet's algorithm of determining the framework version of MSAL."
---

# Overriding target framework

In rare circumstances you might want to override NuGet's algorithm of determining the framework version of MSAL. This is useful if you have a confidential client application and need to target `net5.0-windows10.x` because MSAL uses Windows Forms on these platforms, which leads to build errors in some environments (e.g., Azure Functions).

Please see the [`TfmOverride`](https://github.com/bgavrilMS/TfmOverride) project for a sample that shows how to implement the requirement.