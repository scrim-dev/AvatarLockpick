@echo off
setlocal EnableExtensions
title AvatarLockpick Builder

set "MAIN_PROJ=AvatarLockpick\AvatarLockpick.csproj"
set "SVC_PROJ=AvatarLockpick.Service\AvatarLockpick.Service.csproj"
set "VERSION_TOOL=Tools\VersionManager\VersionManager.csproj"
set "VERSION_NUGET_CONFIG=Tools\VersionManager\NuGet.Config"
set "INSTALLER_DIR=AvatarLockpick.Installer"
set "INSTALLER_SRC_DIR=%INSTALLER_DIR%\src"
set "OUT_ROOT=publish"
set "OUT_WIN=%OUT_ROOT%\win-x64"
set "OUT_SVC=%OUT_ROOT%\service-win-x64"
set "OUT_LINUX=%OUT_ROOT%\linux-x64"
set "LAYOUT=%OUT_ROOT%\AvatarLockpick-win-x64"
set "BINARY_DIR=_binary"
set "RELEASES_DIR=releases"
set "INSTALLER_LOG=%RELEASES_DIR%\installer-build.log"
set "INSTALLER_STEP_LOG=%RELEASES_DIR%\installer-build-step.log"
set "DEV_MODE=false"
set "INSTALLER_PACKAGE_URL=https://raw.githubusercontent.com/scrim-dev/AvatarLockpick/master/_binary/AvatarLockpick-win-x64.zip"

if /i "%~1"=="--dev" (
    set "DEV_MODE=true"
    set "INSTALLER_PACKAGE_URL=https://file-examples.com/wp-content/storage/2017/02/zip_10MB.zip"
)
set "INSTALLER_LDFLAGS=-H=windowsgui -s -w -X main.devMode=%DEV_MODE% -X main.packageURL=%INSTALLER_PACKAGE_URL%"

if "%DEV_MODE%"=="true" (
    echo.
    echo  DEV MODE enabled. Installer download URL: %INSTALLER_PACKAGE_URL%
)

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
mkdir "%LAYOUT%\UI_Fixer" "%LAYOUT%\libs" >nul
xcopy /e /i /y /q "%OUT_WIN%\*" "%LAYOUT%\" >nul
xcopy /e /i /y /q "%OUT_SVC%\*" "%LAYOUT%\" >nul
xcopy /e /i /y /q "AvatarLockpick\libs\*" "%LAYOUT%\libs\" >nul
if exist "AvatarLockpick.Cleaner\UI_Cleaner.bat" copy /y "AvatarLockpick.Cleaner\UI_Cleaner.bat" "%LAYOUT%\UI_Fixer\UI_Cleaner.bat" >nul
copy /y "version.txt" "%LAYOUT%\version.txt" >nul
if not exist "%BINARY_DIR%" mkdir "%BINARY_DIR%"
if not exist "%RELEASES_DIR%" mkdir "%RELEASES_DIR%"
echo  Removing old versioned packages from %BINARY_DIR%...
del /q "%BINARY_DIR%\AvatarLockpick-*-win-x64.zip" >nul 2>nul
powershell -NoProfile -Command "Compress-Archive -Path '%LAYOUT%\*' -DestinationPath '%BINARY_DIR%\AvatarLockpick-win-x64.zip' -Force"
if errorlevel 1 goto :ERROR
copy /y "%BINARY_DIR%\AvatarLockpick-win-x64.zip" "%BINARY_DIR%\AvatarLockpick-%VERSION%-win-x64.zip" >nul

echo.
echo  [4/4] Building Fyne installer...
echo  Installer log: %INSTALLER_LOG%
echo  This can take several minutes the first time because Fyne compiles native Windows UI dependencies.
echo  For live details in another PowerShell window:
echo    Get-Content -Wait "%CD%\%INSTALLER_LOG%"
if exist "%INSTALLER_LOG%" del /q "%INSTALLER_LOG%"
if exist "%INSTALLER_STEP_LOG%" del /q "%INSTALLER_STEP_LOG%"
pushd "%INSTALLER_SRC_DIR%"
echo  Capturing installer build details to ..\..\%INSTALLER_LOG%
echo AvatarLockpick installer build log > "..\..\%INSTALLER_LOG%"
echo Version: %VERSION% >> "..\..\%INSTALLER_LOG%"
echo Dev mode: %DEV_MODE% >> "..\..\%INSTALLER_LOG%"
echo Package URL: %INSTALLER_PACKAGE_URL% >> "..\..\%INSTALLER_LOG%"
echo Started: %DATE% %TIME% >> "..\..\%INSTALLER_LOG%"
echo. >> "..\..\%INSTALLER_LOG%"

echo.
echo  [4/4] Checking Go toolchain...
go version
if errorlevel 1 (popd & goto :ERROR)
go version >> "..\..\%INSTALLER_LOG%" 2>&1

echo.
echo  [4/4] Checking C compiler...
where gcc
if errorlevel 1 (
    echo  [ERROR] gcc.exe was not found on PATH. Add your MinGW-w64 bin folder, for example C:\mingw64\bin.
    echo [ERROR] gcc.exe was not found on PATH. >> "..\..\%INSTALLER_LOG%"
    popd
    goto :ERROR
)
where gcc >> "..\..\%INSTALLER_LOG%" 2>&1
gcc --version
gcc --version >> "..\..\%INSTALLER_LOG%" 2>&1

echo.
echo  [4/4] Preparing installer icon resource...
where windres
if errorlevel 1 (
    echo  [ERROR] windres.exe was not found on PATH. Install a full MinGW-w64 toolchain that includes binutils.
    echo [ERROR] windres.exe was not found on PATH. >> "..\..\%INSTALLER_LOG%"
    popd
    goto :ERROR
)
windres -O coff -F pe-x86-64 -i installer.rc -o installer.syso >> "..\..\%INSTALLER_LOG%" 2>&1
if errorlevel 1 (
    echo  [ERROR] Failed to compile installer icon resource. See %INSTALLER_LOG%.
    popd
    goto :ERROR
)

echo.
echo  [4/4] Go build environment...
for /f %%G in ('go env CGO_ENABLED') do set "CGO_ENABLED=%%G"
go env CGO_ENABLED CC CXX GOOS GOARCH
go env CGO_ENABLED CC CXX GOOS GOARCH >> "..\..\%INSTALLER_LOG%" 2>&1
if not "%CGO_ENABLED%"=="1" (
    echo  [ERROR] Fyne's Windows renderer requires CGO and a C compiler. Install MinGW-w64 and set CGO_ENABLED=1.
    echo [ERROR] CGO_ENABLED is not 1. >> "..\..\%INSTALLER_LOG%"
    popd
    goto :ERROR
)

echo.
echo  [4/4] Restoring Go modules...
echo Running: go mod tidy
echo. >> "..\..\%INSTALLER_LOG%"
echo === go mod tidy === >> "..\..\%INSTALLER_LOG%"
go mod tidy > "..\..\%INSTALLER_STEP_LOG%" 2>&1
set "GO_STEP_EXIT=%ERRORLEVEL%"
type "..\..\%INSTALLER_STEP_LOG%"
type "..\..\%INSTALLER_STEP_LOG%" >> "..\..\%INSTALLER_LOG%"
del /q "..\..\%INSTALLER_STEP_LOG%" >nul 2>nul
if not "%GO_STEP_EXIT%"=="0" (
    echo  [ERROR] Go module restore failed. See %INSTALLER_LOG%.
    popd
    goto :ERROR
)

echo.
echo  [4/4] Compiling installer executable...
echo  Verbose Go output is written to %INSTALLER_LOG%.
echo  If it looks quiet here, the build may still be working; watch the log with the command shown above.
echo Running: go build -v -x -trimpath -ldflags="%INSTALLER_LDFLAGS%"
echo. >> "..\..\%INSTALLER_LOG%"
echo === go build === >> "..\..\%INSTALLER_LOG%"
go build -v -x -trimpath -ldflags="%INSTALLER_LDFLAGS%" -o "..\..\%RELEASES_DIR%\AvatarLockpick-Installer.exe" . >> "..\..\%INSTALLER_LOG%" 2>&1
set "GO_BUILD_EXIT=%ERRORLEVEL%"
if not "%GO_BUILD_EXIT%"=="0" (
    echo  [ERROR] Installer compile failed. See %INSTALLER_LOG%.
    echo  Last 40 log lines:
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-Content '..\..\%INSTALLER_LOG%' -Tail 40"
    popd
    goto :ERROR
)
popd

echo.
echo  Creating Linux build (non-fatal)...
dotnet publish "%MAIN_PROJ%" -c Release -r linux-x64 --no-self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OUT_LINUX%"
if errorlevel 1 (
    echo  Linux build failed, continuing because it is not managed by the Windows installer.
) else (
    echo  Packaging Linux release...
    del /q "%RELEASES_DIR%\AvatarLockpick-*-linux-x64.zip" >nul 2>nul
    powershell -NoProfile -Command "Compress-Archive -Path '%OUT_LINUX%\*' -DestinationPath '%RELEASES_DIR%\AvatarLockpick-linux-x64.zip' -Force"
    if errorlevel 1 (
        echo  Linux zip packaging failed, continuing because it is not managed by the Windows installer.
    ) else (
        copy /y "%RELEASES_DIR%\AvatarLockpick-linux-x64.zip" "%RELEASES_DIR%\AvatarLockpick-%VERSION%-linux-x64.zip" >nul
    )
)

echo.
echo  Done: %BINARY_DIR%\AvatarLockpick-win-x64.zip
echo  Installer: %RELEASES_DIR%\AvatarLockpick-Installer.exe
echo  Linux: %RELEASES_DIR%\AvatarLockpick-linux-x64.zip
exit /b 0

:ERROR
echo.
echo  Build failed.
exit /b 1
