; Installer script and code
#define MyAppName "AvatarLockpick"
#define MyAppVersion "2.3"
#define MyAppPublisher "Scrimmane"
#define MyAppURL "https://scrim.cc/"
#define MyAppExeName "AvatarLockpick.Revised.exe"
#define DiscordURL "https://discord.com/invite/5fc2BWuFWU"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{C764D916-CF13-4237-9B30-91148F5F95A9}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableDirPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}

; "ArchitecturesAllowed=x64compatible" specifies that Setup cannot run
; on anything but x64 and Windows 11 on Arm.
ArchitecturesAllowed=x64compatible

; "ArchitecturesInstallIn64BitMode=x64compatible" requests that the
; install be done in "64-bit mode" on x64 or Windows 11 on Arm,
; meaning it should use the native 64-bit Program Files directory and
; the 64-bit view of the registry.
ArchitecturesInstallIn64BitMode=x64compatible

DisableProgramGroupPage=yes
InfoBeforeFile=C:\Users\intro\source\repos\AvatarLockpick\AvatarLockpick.Installer\StartUpInfo.txt
InfoAfterFile=C:\Users\intro\source\repos\AvatarLockpick\AvatarLockpick.Installer\ClosingInfo.txt

; FORCE ADMIN: Installing .NET Runtime and writing to HKLM usually requires Admin rights.
PrivilegesRequired=admin

OutputDir=C:\Users\intro\source\repos\AvatarLockpick\AvatarLockpick.Installer\Output
OutputBaseFilename=AvatarLockpick_Setup
SetupIconFile=C:\Users\intro\source\repos\AvatarLockpick\AvatarLockpick.Installer\dl_logo.ico
SolidCompression=yes
WizardStyle=modern dark windows11

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Registry]
; Adds Registry keys under Software\Scrimmane\AvatarLockpick
; "HKA" automatically selects HKLM (if admin) or HKCU (if user), but since we forced admin, this goes to HKLM.
Root: HKA; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Publisher"; ValueData: "{#MyAppPublisher}"; Flags: uninsdeletekey

[Files]
Source: "C:\Users\intro\source\repos\AvatarLockpick\AvatarLockpick.Revised\bin\Release\net8.0-windows10.0.26100.0\publish\win-x64\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\intro\source\repos\AvatarLockpick\AvatarLockpick.Revised\bin\Release\net8.0-windows10.0.26100.0\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: .NET Installer removed from [Files] and moved to [Code] to handle installation logic automatically.

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Define the DotNet 8.0 URL
const
  DotNetUrl = 'https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/8.0.22/windowsdesktop-runtime-8.0.22-win-x64.exe';

var
  DownloadPage: TDownloadWizardPage;

function OnDownloadProgress(const Url, FileName: String; const Progress, ProgressMax: Int64): Boolean;
begin
  if ProgressMax <> 0 then
    Log(Format('  %d of %d bytes done.', [Progress, ProgressMax]))
  else
    Log(Format('  %d bytes done.', [Progress]));
  Result := True;
end;

procedure InitializeWizard;
begin
  DownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), @OnDownloadProgress);
end;

function IsDotNet8DesktopInstalled: Boolean;
var
  Success: Boolean;
  InstallVer: String;
begin
  // Check the registry for .NET Desktop Runtime 8.0 (x64)
  // We check HKLM because we are in 64-bit mode
  // The key 'Microsoft.WindowsDesktop.App' contains subkeys for versions installed
  
  // Method: Check if a key starting with '8.' exists in the sharedfx folder
  Result := RegKeyExists(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App\8.0.22') or
            RegKeyExists(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App\8.0.0') or 
            RegKeyExists(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App\8.0.1') or
            // Generic check for any 8.x version if exact match fails
            RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Updates\.NET\8.0'); 

  // A more robust check for ANY 8.x version:
  if not Result then
  begin
      // If we can't find specific keys, let's assume false and let the installer try to install it. 
      Result := False;
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  ErrorCode: Integer;
begin
  Result := True;

  if (CurPageID = wpReady) then
  begin
    if not IsDotNet8DesktopInstalled then
    begin
      if MsgBox('This application requires the .NET Desktop Runtime 8.0.' + #13#10 + 'Do you want to download and install it now?', mbConfirmation, MB_YESNO) = IDYES then
      begin
        DownloadPage.Clear;
        DownloadPage.Add(DotNetUrl, 'windowsdesktop-runtime-8.0.22-win-x64.exe', '');
        DownloadPage.Show;
        try
          try
            DownloadPage.Download; // This downloads the file
            Result := True;
          except
            SuppressibleMsgBox(AddPeriod(GetExceptionMessage), mbCriticalError, MB_OK, IDOK);
            Result := False;
          end;
        finally
          DownloadPage.Hide;
        end;

        if Result then
        begin
          // Run the installer silently
          ShellExec('runas', ExpandConstant('{tmp}\windowsdesktop-runtime-8.0.22-win-x64.exe'), '/install /quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ErrorCode);
          
          // Check for reboot requirement (1641 or 3010)
          if (ErrorCode = 1641) or (ErrorCode = 3010) then
          begin
             MsgBox('Computer needs to restart to complete .NET installation.', mbInformation, MB_OK);
          end;
        end;
      end
      else
      begin
        MsgBox('The application may not run without .NET 8.0.', mbInformation, MB_OK);
      end;
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ErrorCode: Integer;
begin
  // ssPostInstall triggers immediately after the actual installation (file copying) is complete,
  // but before the final "Finished" wizard page is shown.
  if CurStep = ssPostInstall then
  begin
    if MsgBox('Would you like to join the discord server?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      // Open the URL in the default browser without waiting for it to close
      ShellExec('open', '{#DiscordURL}', '', '', SW_SHOW, ewNoWait, ErrorCode);
    end;
  end;
end;