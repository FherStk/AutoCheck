#!/bin/bash
dotnet build -c Release > /dev/null 2>&1
dotnet bin/Release/net8.0/AutoCheck.Cli.dll $1 $2 $3