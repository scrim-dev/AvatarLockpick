@echo off
:: Set window title and dimensions
title UI Cleaner
mode con: cols=65 lines=18
color 0B

:menu
cls
echo =============================================================
echo                  UI CLEANER BY SCRIM-DEV                     
echo =============================================================
echo.
echo  This tool will force delete the Photino cache directory:
echo  "%LocalAppData%\Photino"
echo.
echo =============================================================
echo.
echo  [1] Start Clean Up
echo  [2] Exit
echo.
set /p choice=" Enter your choice (1-2): "

if "%choice%"=="1" goto clean
if "%choice%"=="2" exit
goto menu

:clean
cls
echo =============================================================
echo                  UI CLEANER BY SCRIM-DEV                     
echo =============================================================
echo.
echo  Target: "%LocalAppData%\Photino"
echo.
echo  Checking folder status...
timeout /t 1 >nul

if exist "%LocalAppData%\Photino" (
    echo  [#] Target folder found.
    echo  [#] Attempting to delete...
    
    :: Attempt deletion and hide standard system output
    rd /s /q "%LocalAppData%\Photino" >nul 2>&1
    
    if exist "%LocalAppData%\Photino" (
        color 0C
        echo.
        echo  [!] ERROR: Could not delete the folder.
        echo             It might be locked or in use by Photino.
    ) else (
        color 0A
        echo.
        echo  [+] SUCCESS: Photino cache has been cleared.
    )
) else (
    color 0E
    echo.
    echo  [*] INFO: Target folder not found. Nothing to clean!
)

echo.
echo =============================================================
echo  Press any key to return to the main menu...
pause >nul
:: Reset UI color to default theme
color 0B
goto menu