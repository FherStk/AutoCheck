@echo off
:: The timeout is used to avoid output mixing between processes
timeout /t 1 /nobreak > NUL 
echo Restarting...
run
@echo on