@echo off

dotnet publish -o ./out -r win-x64 -c Release /p:PublishSingleFile=true --self-contained false
del ".\out\appsettings.json"