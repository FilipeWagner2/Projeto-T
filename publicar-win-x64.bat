@echo off
setlocal

set "PROJECT_DIR=%~dp0"
set "PROJECT_FILE=%PROJECT_DIR%TranslatorTrayApp.csproj"

where dotnet >nul 2>nul
if errorlevel 1 (
    echo Nao encontrei o comando dotnet.
    echo Instale o .NET 8 SDK e tente de novo.
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

dotnet publish "%PROJECT_FILE%" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
pause
