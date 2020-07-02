PowerShell -Command "Set-ExecutionPolicy Unrestricted" >> "%TEMP%\StartupLog.txt" 2>&1
PowerShell -file C:\wsl2_setup.ps1 >> "%TEMP%\StartupLog.txt" 2>&1