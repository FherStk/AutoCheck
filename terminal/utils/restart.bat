@echo off
:: The timeout is used to avoid output mixing between processes
timeout /t 1 /nobreak > NUL 
dotnet build -c Release
dotnet bin/Release/net6.0/AutoCheck.Terminal.dll -nu %1 %2
@echo on