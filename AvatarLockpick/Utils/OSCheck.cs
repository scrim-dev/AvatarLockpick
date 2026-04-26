using System;
using System.Runtime.InteropServices;

namespace AvatarLockpick.Utils
{
    internal class OSCheck
    {
        public static bool IsLinux()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }

        public static bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
    }
}
