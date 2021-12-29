@echo off
:: The timeout is used to avoid output mixing between processes
timeout /t 1 /nobreak > NUL 
run -nu %1 %2
@echo on