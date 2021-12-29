#!/bin/bash
dotnet build -c Release > /dev/null 2>&1
dotnet bin/Release/net6.0/AutoCheck.Web.dll