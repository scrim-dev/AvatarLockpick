@echo off
setlocal EnableExtensions
title AvatarLockpick Builder

set "MAIN_PROJ=AvatarLockpick\AvatarLockpick.csproj"
set "SVC_PROJ=AvatarLockpick.Service\AvatarLockpick.Service.csproj"
set "VERSION_TOOL=Tools\VersionManager\VersionManager.csproj"
set "VERSION_NUGET_CONFIG=Tools\VersionManager\NuGet.Config"
set "INSTALLER_DIR=AvatarLockpick.Installer"
set "OUT_ROOT=publish"
set "OUT_WIN=%OUT_ROOT%\win-x64"
set "OUT_SVC=%OUT_ROOT%\service-win-x64"
set "OUT_LINUX=%OUT_ROOT%\linux-x64"
set "LAYOUT=%OUT_ROOT%\AvatarLockpick-win-x64"
set "BINARY_DIR=_binary"

echo.
echo  Updating calendar version...
dotnet restore "%VERSION_TOOL%" --configfile "%VERSION_NUGET_CONFIG%"
if errorlevel 1 goto :ERROR
dotnet run --project "%VERSION_TOOL%" -c Release --no-restore
if errorlevel 1 goto :ERROR
set /p VERSION=<version.txt

echo.
echo  [1/4] Publishing AvatarLockpick %VERSION%...
dotnet publish "%MAIN_PROJ%" -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OUT_WIN%"
if errorlevel 1 goto :ERROR

echo.
echo  [2/4] Publishing service...
dotnet publish "%SVC_PROJ%" -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OUT_SVC%"
if errorlevel 1 goto :ERROR

echo.
echo  [3/4] Creating distributable package layout...
if exist "%LAYOUT%" rmdir /s /q "%LAYOUT%"
mkdir "%LAYOUT%\GUI full tool" "%LAYOUT%\libs" >nul
xcopy /e /i /y /q "%OUT_WIN%\*" "%LAYOUT%\" >nul
xcopy /e /i /y /q "%OUT_SVC%\*" "%LAYOUT%\" >nul
xcopy /e /i /y /q "AvatarLockpick\libs\*" "%LAYOUT%\libs\" >nul
if exist "AvatarLockpick.Cleaner\UI_Cleaner.bat" copy /y "AvatarLockpick.Cleaner\UI_Cleaner.bat" "%LAYOUT%\GUI full tool\UI_Cleaner.bat" >nul
copy /y "version.txt" "%LAYOUT%\version.txt" >nul
if not exist "%BINARY_DIR%" mkdir "%BINARY_DIR%"
powershell -NoProfile -Command "Compress-Archive -Path '%LAYOUT%\*' -DestinationPath '%BINARY_DIR%\AvatarLockpick-win-x64.zip' -Force"
if errorlevel 1 goto :ERROR
copy /y "%BINARY_DIR%\AvatarLockpick-win-x64.zip" "%BINARY_DIR%\AvatarLockpick-%VERSION%-win-x64.zip" >nul

echo.
echo  [4/4] Building Fyne installer...
pushd "%INSTALLER_DIR%"
for /f %%G in ('go env CGO_ENABLED') do set "CGO_ENABLED=%%G"
if not "%CGO_ENABLED%"=="1" (
    echo  [ERROR] Fyne's Windows renderer requires CGO and a C compiler. Install MinGW-w64 and set CGO_ENABLED=1.
    popd
    goto :ERROR
)
go mod tidy
if errorlevel 1 (popd & goto :ERROR)
go build -trimpath -ldflags="-s -w" -o "..\%BINARY_DIR%\AvatarLockpick-Installer.exe" .
if errorlevel 1 (popd & goto :ERROR)
popd

echo.
echo  Creating Linux build (non-fatal)...
dotnet publish "%MAIN_PROJ%" -c Release -r linux-x64 --no-self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OUT_LINUX%"

echo.
echo  Done: %BINARY_DIR%\AvatarLockpick-win-x64.zip
exit /b 0

:ERROR
echo.
echo  Build failed.
exit /b 1
