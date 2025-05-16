using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarLockpick.Revised.Utils
{
    internal class AppFolders
    {
        private static readonly string LocalLowAppData = Environment.GetFolderPath
            (Environment.SpecialFolder.LocalApplicationData).Replace("Local", "LocalLow");
        public static string DataLowFolder { get; private set; } = $"{LocalLowAppData}\\alp_data";
        public static void Load()
        {
            if (!Directory.Exists(DataLowFolder))
            {
                try { Directory.Delete(DataLowFolder); } catch { }
            }
        }
    }
}
