@echo off
setlocal

set "PROJECT_DIR=%~dp0"
set "PROJECT_FILE=%PROJECT_DIR%TranslatorTrayApp.csproj"
set "APP_EXE=%PROJECT_DIR%bin\Release\net8.0-windows\TranslatorTrayApp.exe"

if exist "%APP_EXE%" (
    start "" "%APP_EXE%"
    exit /b 0
)

where dotnet >nul 2>nul
if errorlevel 1 (
    echo Nao encontrei o comando dotnet.
    echo Instale o .NET 8 Runtime ou SDK e tente de novo.
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

dotnet run -c Release --project "%PROJECT_FILE%"
