@echo off
for %%f in (*.sln) do set "SOLUTION_FILE=%%f"
dotnet build "%SOLUTION_FILE%" -c Release /p:Platform="Any CPU" /p:PlatformTarget=x64
timeout /t 5
iscc "AvatarLockpick.Installer\Main.iss"
pause