---
author: cilwerner
manager: 
ms.author: cwerner
ms.date: 05/22/2025
ms.service: msal
ms.subservice: msal-dotnet
ms.reviewer: 
ms.topic: include
ms.custom: 
#Customer intent: 
# Purpose:
# Ingested by Microsoft identity platform articles in /articles/active-directory/develop/* that document the error handling Conditional Access and claims challenges for the different platforms.
---
## Conditional Access and claims challenges

When getting tokens silently, your application may receive errors when a [Conditional Access claims challenge](/azure/active-directory/develop/v2-conditional-access-dev-guide) such as MFA policy is required by an API you're trying to access.

The pattern for handling this error is to interactively acquire a token using MSAL. This prompts the user and gives them the opportunity to satisfy the required Conditional Access policy.

In certain cases when calling an API requiring Conditional Access, you can receive a claims challenge in the error from the API. For instance if the Conditional Access policy is to have a managed device (Intune) the error will be something like [AADSTS53000: Your device is required to be managed to access this resource](/azure/active-directory/develop/reference-error-codes) or something similar. In this case, you can pass the claims in the acquire token call so that the user is prompted to satisfy the appropriate policy.
