; Installer script and code
#define MyAppName "AvatarLockpick"
#define MyAppVersion "2.3"
#define MyAppPublisher "Scrimmane"
#define MyAppURL "https://scrim.cc/"
#define MyAppExeName "AvatarLockpick.Revised.exe"
#define DiscordURL "https://discord.com/invite/5fc2BWuFWU"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
AppId={{C764D916-CF13-4237-9B30-91148F5F95A9}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; --- CHANGE 1: Install to User's AppData Programs folder ---
; {userpf} resolves to C:\Users\Username\AppData\Local\Programs
DefaultDirName={userpf}\{#MyAppName}

DisableDirPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}

; "ArchitecturesAllowed=x64compatible" specifies that Setup cannot run on anything but x64/Arm64.
ArchitecturesAllowed=x64compatible

; "ArchitecturesInstallIn64BitMode=x64compatible" requests 64-bit mode.
ArchitecturesInstallIn64BitMode=x64compatible

DisableProgramGroupPage=yes
InfoBeforeFile=C:\Users\intro\source\repos\AvatarLockpick\AvatarLockpick.Installer\StartUpInfo.txt
InfoAfterFile=C:\Users\intro\source\repos\AvatarLockpick\AvatarLockpick.Installer\ClosingInfo.txt

; FORCE ADMIN: Required to install the .NET Runtime.
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

; --- CHANGE 2: Fix "Access Denied" for Logs ---
; Even though we are Admin, we explicitly grant the "Users" group modification rights
; to the installation folder so the app can write its logs.
[Dirs]
Name: "{app}"; Permissions: users-modify

[Registry]
; Adds Registry keys. using HKA (HKLM because of admin privileges)
Root: HKA; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Publisher"; ValueData: "{#MyAppPublisher}"; Flags: uninsdeletekey

[Files]
Source: "C:\Users\intro\source\repos\AvatarLockpick\AvatarLockpick.Revised\bin\Release\net8.0-windows10.0.26100.0\publish\win-x64\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\intro\source\repos\AvatarLockpick\AvatarLockpick.Revised\bin\Release\net8.0-windows10.0.26100.0\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; --- CHANGE 3: Create shortcuts for the USER only, not Global ---
Name: "{userprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{userdesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

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
  // We check HKLM because we are in 64-bit mode (ArchitecturesInstallIn64BitMode=x64compatible)
  Result := RegKeyExists(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App\8.0.22') or
            RegKeyExists(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App\8.0.0') or 
            RegKeyExists(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App\8.0.1') or
            RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Updates\.NET\8.0'); 

  if not Result then
  begin
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
            DownloadPage.Download;
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
  if CurStep = ssPostInstall then
  begin
    if MsgBox('Would you like to join the discord server?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', '{#DiscordURL}', '', '', SW_SHOW, ewNoWait, ErrorCode);
    end;
  end;
end;