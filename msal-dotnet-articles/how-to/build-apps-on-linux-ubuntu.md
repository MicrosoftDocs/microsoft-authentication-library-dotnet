# Test App for Linux
Create a console app for linux testing. Right now, it tests [#2839](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/2839)
```CSharp
class Program
    {
        public static string ClientID = "your client id"; //msidentity-samples-testing tenant
        public static string[] Scopes = { "User.Read" };
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var pcaBuilder = PublicClientApplicationBuilder.Create(ClientID)
                .WithRedirectUri("http://localhost")
                .Build();

            AcquireTokenInteractiveParameterBuilder atparamBuilder = pcaBuilder.AcquireTokenInteractive(Scopes);

            AuthenticationResult authenticationResult = atparamBuilder.ExecuteAsync().GetAwaiter().GetResult();
            System.Console.WriteLine(authenticationResult.AccessToken);
        }
    }
```

# How to setup
In a Ubuntu machine
- Download VS Code
- Copy the files from the download folder to an "App" folder.
- Download the NuGet package in ~/LocalNuget folder.

# How to build
From the VS Code terminal
- Go to the "App" folder
- Run command
```dotnet add package Microsoft.Identity.Client --prerelease -s ~/LocalNuget```
This will add the latest package to the project
- Run command
```dotnet build```
This will build the app in debug mode

# How to run
From the Powershell terminal
- Got to "App"/bin/Debug/net6 folder
- Run command
```dotnet TestApp.dll```
This runs the app
- To test in Sudo mode, run the following command
```sudo dotnet TestApp.dll```
