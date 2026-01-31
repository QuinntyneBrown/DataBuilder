@echo off
setlocal

set "CLI_PROJECT=src\DataBuilder.Cli\DataBuilder.Cli.csproj"
set "NUPKG_PATH=src\DataBuilder.Cli\nupkg"

pushd "%~dp0..\.."

echo Building DataBuilder.Cli in Release mode...
dotnet build "%CLI_PROJECT%" -c Release
if errorlevel 1 (
    echo Build failed.
    popd
    exit /b 1
)

echo Packing as tool...
dotnet pack "%CLI_PROJECT%" -c Release
if errorlevel 1 (
    echo Pack failed.
    popd
    exit /b 1
)

echo Uninstalling existing tool (if installed)...
dotnet tool uninstall -g QuinntyneBrown.DataBuilder.Cli 2>nul

echo Installing tool from local package...
dotnet tool install -g QuinntyneBrown.DataBuilder.Cli --add-source "%NUPKG_PATH%"
if errorlevel 1 (
    echo Install failed.
    popd
    exit /b 1
)

echo.
echo DataBuilder.Cli installed successfully.
echo Run 'db --help' to get started.

popd
