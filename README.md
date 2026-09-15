# FlowOps API

All six projects target .NET 9. Install a .NET 9 SDK (9.0.200 or later).
The solution's global.json selects a stable .NET 9 SDK.

## Visual Studio

Use Visual Studio 2022 17.12 or later with the ASP.NET and web development
workload. Visual Studio 17.7 cannot build these projects even when the .NET 9
SDK is installed separately. Open Visual Studio Installer, update Community
2022, then reopen FlowOps.sln and rebuild.

Do not change TargetFramework to net7.0 to work around NETSDK1045: the project
uses .NET 9 ASP.NET Core and Entity Framework Core dependencies.

## Command-line build

From this directory:

```powershell
dotnet --version
dotnet build FlowOps.sln
dotnet run --project FlowOps.Api --launch-profile https
```

The CLI uses its own MSBuild and can build with the installed .NET 9 SDK
while Visual Studio is being updated. API startup requires PostgreSQL because
database migrations run before the server starts. The HTTPS launch profile
listens on port 7281, matching the frontend Vite proxy.
