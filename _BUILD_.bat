:: Full Builder
@echo off

title ALP Builder

set "MAIN_PROJ=AvatarLockpick\AvatarLockpick.csproj"
set "SVC_PROJ=AvatarLockpick.Service\AvatarLockpick.Service.csproj"

set "OUT_WIN=publish\win-x64"
set "OUT_SVC=publish\service-win-x64"
set "OUT_LINUX=publish\linux-x64"

set "LIBS_SRC=AvatarLockpick\libs"
set "CLEANER_BAT=AvatarLockpick.Cleaner\UI_Cleaner.bat"

set "VERSION=?.?"
if exist version.txt set /p VERSION=<version.txt

echo.
echo  ALP Builder  v%VERSION%
echo.

:: ---------------------------------------------------------------
echo  ^>^> [1/3] Main App -- Win x64 Release...
echo.
dotnet publish "%MAIN_PROJ%" -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OUT_WIN%"
if errorlevel 1 (
    echo.
    echo  [ERROR] Main App Win x64 build FAILED. See output above.
    echo.
    goto DONE
)
call :COPY_MAIN_LIBS "%OUT_WIN%"
call :COPY_CLEANER "%OUT_WIN%"

:: ---------------------------------------------------------------
echo.
echo  ^>^> [2/3] Service -- Win x64 Release...
echo.
dotnet publish "%SVC_PROJ%" -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OUT_SVC%"
if errorlevel 1 (
    echo.
    echo  [ERROR] Service Win x64 build FAILED. See output above.
    echo.
    goto DONE
)

:: ---------------------------------------------------------------
echo.
echo  ^>^> [3/3] Main App -- Linux x64 Release...
echo.
dotnet publish "%MAIN_PROJ%" -c Release -r linux-x64 --no-self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OUT_LINUX%"
if errorlevel 1 (
    echo.
    echo  [WARN] Linux x64 build FAILED. See output above.
    echo.
)

:DONE
echo.
echo  Done.
echo.
pause
endlocal
exit /b 0

:: ============================================================
:COPY_MAIN_LIBS
:: %~1 = destination publish dir (will create %~1\libs\)
echo  ^>^> Copying ADB libs to %~1\libs\...
if not exist "%~1\libs" mkdir "%~1\libs"
for %%F in (adb.exe AdbWinApi.dll AdbWinUsbApi.dll) do (
    if exist "%LIBS_SRC%\%%F" (
        xcopy /y /q "%LIBS_SRC%\%%F" "%~1\libs\" >nul
    ) else (
        echo  [WARN] %LIBS_SRC%\%%F not found, skipping.
    )
)
echo  ^>^> ADB libs copied.
exit /b 0

:: ============================================================
:COPY_CLEANER
:: %~1 = destination publish dir
if exist "%CLEANER_BAT%" (
    copy /y "%CLEANER_BAT%" "%~1\ALP_UI_Cleaner.bat" >nul
    echo  ^>^> UI_Cleaner.bat copied to %~1 as ALP_UI_Cleaner.bat.
) else (
    echo  [WARN] %CLEANER_BAT% not found, skipping.
)
exit /b 0