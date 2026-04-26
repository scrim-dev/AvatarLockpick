@echo off
for %%f in (*.sln) do set "SOLUTION_FILE=%%f"
dotnet build "%SOLUTION_FILE%" -c Release /p:Platform="Any CPU" /p:PlatformTarget=x64
dotnet publish "%SOLUTION_FILE%" -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o linux_release
timeout /t 5
iscc "AvatarLockpick.Installer\InstallerScript.iss"
pause