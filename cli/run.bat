@Echo off
dotnet build -c Release
@Echo on
dotnet bin/Release/net6.0/AutoCheck.Cli.dll %1 %2 %3