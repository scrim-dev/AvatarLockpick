# VersionManager

`dotnet run --project Tools/VersionManager` updates the application and service source versions,
assembly/file versions, `version.txt`, and `AvatarLockpick/version.json`.

Versions use `YYYY.MM.DD-BuildCount`. The counter increments on every build and only starts at
`1` when the year changes.
