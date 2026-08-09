# AvatarLockpick Installer

Go source lives in `src`. The installer uses `src/Ins_icon.ico` for both the Fyne window icon
and the compiled Windows executable icon.

The Fyne installer installs AvatarLockpick to `%LOCALAPPDATA%\Programs\AvatarLockpick`
(usually `C:\Users\<username>\AppData\Local\Programs\AvatarLockpick`). It always replaces the
Start Menu shortcut and can create a desktop shortcut when the user selects that option.

It checks the repository `version.txt`, compares it to the installed executable's version, checks
whether the matching GitHub Release exists, and warns before installing an unpublished pre-release.

Run `build.bat` from the repository root. Building the installer requires Go, CGO enabled, and a
C compiler such as MinGW-w64, because Fyne's Windows renderer uses OpenGL.

Use `build.bat --dev` to build a dev-mode installer. Dev mode downloads
`https://file-examples.com/wp-content/storage/2017/02/zip_10MB.zip` with a Firefox User-Agent to test the
installer download path, then stops without installing files or changing shortcuts.

Build outputs:

- Windows installer-managed app update zip: `..\_binary\AvatarLockpick-win-x64.zip`
- Installer executable: `..\releases\AvatarLockpick-Installer.exe`
- Linux release zip, not managed by the installer: `..\releases\AvatarLockpick-linux-x64.zip`
- Installer build log: `..\releases\installer-build.log`
