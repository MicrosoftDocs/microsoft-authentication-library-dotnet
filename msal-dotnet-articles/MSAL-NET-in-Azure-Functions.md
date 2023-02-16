When using MSAL.NET in Azure functions, it can happen that libraries are not copied to the directory.

You can add `<_FunctionsSkipCleanOutput>true</_FunctionsSkipCleanOutput>` to your .csproj file to prevent that.

See details in Azure/azure-functions-host#5894

See also how to build [Azure functions with Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki/Azure-Functions)