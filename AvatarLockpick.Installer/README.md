# AvatarLockpick Installer

The Fyne installer installs AvatarLockpick to `%LOCALAPPDATA%\Programs\AvatarLockpick`
(usually `C:\Users\<username>\AppData\Local\Programs\AvatarLockpick`). It always replaces the
Start Menu shortcut and can create a desktop shortcut when the user selects that option.

It checks the repository `version.txt`, compares it to the installed executable's version, checks
whether the matching GitHub Release exists, and warns before installing an unpublished pre-release.

Run `build.bat` from the repository root. Building the installer requires Go, CGO enabled, and a
C compiler such as MinGW-w64, because Fyne's Windows renderer uses OpenGL.
