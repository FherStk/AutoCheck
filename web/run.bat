@Echo off
dotnet build -c Release
@Echo on
dotnet bin/Release/net10.0/AutoCheck.Web.dll --urls=http://0.0.0.0:80/
