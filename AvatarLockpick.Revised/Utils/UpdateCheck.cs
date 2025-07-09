using System;
using System.Net.Http;
using System.Reflection;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AvatarLockpick.Revised.Utils
{
    public static class VersionChecker
    {
        private static readonly HttpClient _httpClient = new();

        // GitHub raw content URL for version file
        private const string VersionFileUrl = "https://raw.githubusercontent.com/scrim-dev/AvatarLockpick/refs/heads/master/version.txt";

        public static void CheckForUpdates()
        {
            try
            {
                var result = CompareVersions();

                switch (result.Status)
                {
                    case VersionStatus.NewVersionAvailable:
                        AppLog.Warn("UPDCheck", $"UPDATE AVAILABLE: {result.CurrentVersion} → {result.RemoteVersion}");
                        MessageBoxUtils.ShowQuestion($"A new update is available!\n\nAvatarLockpick: {result.CurrentVersion} → " +
                            $"AvatarLockpick: {result.RemoteVersion}\n\nDo you want to Update?", "Update", delegate
                        {
                            //All zips will be called LockpickApp.zip now
                            URLStuff.OpenUrl($"https://github.com/scrim-dev/AvatarLockpick/releases/download/v{result.RemoteVersion}/LockpickApp.zip");
                        });

                        break;

                    case VersionStatus.UpToDate:
                        AppLog.Log("UPDCheck", "You have the latest version");
                        break;

                    case VersionStatus.LocalIsNewer:
                        AppLog.Warn("UPDCheck", "You're running a newer version than published");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Version check failed: {ex.Message}");
            }
        }

        public static VersionComparisonResult CompareVersions()
        {
            var currentVersion = GetCurrentVersion();
            var remoteVersionText = FetchRemoteVersion();

            if (string.IsNullOrWhiteSpace(remoteVersionText))
            {
                return new VersionComparisonResult(VersionStatus.FailedToCheck, currentVersion);
            }

            if (!Version.TryParse(remoteVersionText.Trim(), out var remoteVersion))
            {
                return new VersionComparisonResult(
                    VersionStatus.InvalidRemoteVersion,
                    currentVersion,
                    remoteVersionStr: remoteVersionText
                );
            }

            var status = remoteVersion > currentVersion ? VersionStatus.NewVersionAvailable
                        : remoteVersion < currentVersion ? VersionStatus.LocalIsNewer
                        : VersionStatus.UpToDate;

            return new VersionComparisonResult(
                status,
                currentVersion,
                remoteVersion
            );
        }

        public static Version ?GetCurrentVersion()
        {
            // Get version from Program.AppVersion (assuming it's a string property)
            if (!string.IsNullOrWhiteSpace(Program.AppVersion) &&
                Version.TryParse(Program.AppVersion, out var version))
            {
                return version;
            }

            /*// Fallback to assembly version
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return new Version(versionInfo.FileVersion ?? "1.0.0.0");*/
            return null;
        }

        private static string FetchRemoteVersion()
        {
            try
            {
                // Add headers to prevent GitHub caching
                _httpClient.DefaultRequestHeaders.CacheControl = new()
                {
                    NoCache = true,
                    NoStore = true
                };

                // Using synchronous GetString instead of async
                return _httpClient.GetStringAsync(VersionFileUrl).GetAwaiter().GetResult();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Failed to fetch version: {ex.Message}");
                return null;
            }
        }
    }

    public class VersionComparisonResult
    {
        public VersionStatus Status { get; }
        public Version CurrentVersion { get; }
        public Version RemoteVersion { get; }
        public string RemoteVersionStr { get; }

        public VersionComparisonResult(VersionStatus status, Version currentVersion)
        {
            Status = status;
            CurrentVersion = currentVersion;
        }

        public VersionComparisonResult(VersionStatus status, Version currentVersion, Version remoteVersion)
            : this(status, currentVersion)
        {
            RemoteVersion = remoteVersion;
        }

        public VersionComparisonResult(VersionStatus status, Version currentVersion, string remoteVersionStr)
            : this(status, currentVersion)
        {
            RemoteVersionStr = remoteVersionStr;
        }
    }

    public enum VersionStatus
    {
        UpToDate,
        NewVersionAvailable,
        LocalIsNewer,
        FailedToCheck,
        InvalidRemoteVersion
    }
}