@echo off
setlocal

set SCRIPT_DIR=%~dp0
set PROJECT_PATH=%SCRIPT_DIR%ArtPrompter\ArtPrompter.csproj

dotnet publish "%PROJECT_PATH%" -c Release ^
  -p:PublishSingleFile=true ^
  -p:SelfContained=true ^
  -p:PublishTrimmed=false ^
  -p:PublishReadyToRun=true

echo Publish output: %SCRIPT_DIR%ArtPrompter\bin\Release\net8.0\win-x64\publish\
endlocal
