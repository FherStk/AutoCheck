#!/bin/bash
dotnet build -c Release > /dev/null 2>&1
dotnet bin/Release/net10.0/AutoCheck.Web.dll --urls=http://0.0.0.0:80/
