@echo off
:: The timeout is used to avoid output mixing between processes
timeout /t 1 /nobreak > NUL 
dotnet run -nu %1
@echo on