PowerShell -Command "Set-ExecutionPolicy Unrestricted" >> "%TEMP%\StartupLog.txt" 2>&1
PowerShell C:\wsl2_setup.ps1 >> "%TEMP%\StartupLog.txt" 2>&1