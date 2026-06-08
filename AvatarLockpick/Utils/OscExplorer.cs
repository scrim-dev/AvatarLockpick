using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace AvatarLockpick.Utils
{
    internal class OscExplorer
    {
        public static string CurrentUserId { get; set; } = "";
        public static string CurrentAvatarId { get; set; } = "";

        private static string GetOscAvatarPath(string userId, string avatarId)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow", "VRChat", "VRChat",
                "OSC", userId, "Avatars", avatarId + ".json");
        }

        private static string GetAvatarDataPath(string userId, string avatarId)
        {
            if (!string.IsNullOrEmpty(AvatarUnlocker.WindowsCustomPath))
                return Path.Combine(AvatarUnlocker.WindowsCustomPath, "LocalAvatarData", userId, avatarId);

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow", "VRChat", "VRChat",
                "LocalAvatarData", userId, avatarId);
        }

        public static JObject LoadExplorerData(string userId, string avatarId)
        {
            var result = new JObject();
            result["userId"] = userId;
            result["avatarId"] = avatarId;

            string oscPath = GetOscAvatarPath(userId, avatarId);
            bool oscExists = File.Exists(oscPath);
            result["oscPath"] = oscPath;
            result["oscExists"] = oscExists;

            JArray oscParams = new JArray();
            if (oscExists)
            {
                try
                {
                    var oscDoc = JObject.Parse(File.ReadAllText(oscPath));
                    result["oscName"] = oscDoc["name"]?.ToString() ?? "";
                    result["oscId"] = oscDoc["id"]?.ToString() ?? "";
                    oscParams = oscDoc["parameters"] as JArray ?? new JArray();
                }
                catch (Exception ex)
                {
                    result["oscError"] = ex.Message;
                    AppLog.Warn("OscExplorer", $"Error reading OSC file: {ex.Message}");
                }
            }

            string avatarPath = GetAvatarDataPath(userId, avatarId);
            bool avatarExists = File.Exists(avatarPath);
            result["avatarPath"] = avatarPath;
            result["avatarExists"] = avatarExists;

            var avatarParamMap = new Dictionary<string, JToken?>(StringComparer.OrdinalIgnoreCase);
            if (avatarExists)
            {
                try
                {
                    string content = File.ReadAllText(avatarPath);
                    JArray? animParams = null;
                    try
                    {
                        var avatarObj = JObject.Parse(content);
                        animParams = avatarObj["animationParameters"] as JArray;
                    }
                    catch
                    {
                        try { animParams = JArray.Parse(content); } catch { }
                    }

                    if (animParams != null)
                    {
                        foreach (JObject param in animParams.OfType<JObject>())
                        {
                            string name = param["name"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(name))
                                avatarParamMap[name] = param["value"];
                        }
                    }
                }
                catch (Exception ex)
                {
                    result["avatarError"] = ex.Message;
                    AppLog.Warn("OscExplorer", $"Error reading avatar file: {ex.Message}");
                }
            }

            var mergedParams = new JArray();
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (JObject oscParam in oscParams.OfType<JObject>())
            {
                string name = oscParam["name"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(name)) continue;
                seenNames.Add(name);

                bool inAvatar = avatarParamMap.TryGetValue(name, out JToken? avatarValue);
                string oscType = oscParam["input"]?["type"]?.ToString()
                    ?? oscParam["output"]?["type"]?.ToString()
                    ?? "Float";

                mergedParams.Add(new JObject
                {
                    ["name"] = name,
                    ["oscType"] = oscType,
                    ["oscInputAddress"] = oscParam["input"]?["address"]?.ToString() ?? $"/avatar/parameters/{name}",
                    ["oscOutputAddress"] = oscParam["output"]?["address"]?.ToString() ?? $"/avatar/parameters/{name}",
                    ["inOsc"] = true,
                    ["inAvatar"] = inAvatar,
                    ["currentValue"] = inAvatar ? avatarValue : JValue.CreateNull(),
                    ["status"] = inAvatar ? "matched" : "osc-only"
                });
            }

            foreach (var kv in avatarParamMap)
            {
                if (seenNames.Contains(kv.Key)) continue;
                mergedParams.Add(new JObject
                {
                    ["name"] = kv.Key,
                    ["oscType"] = "Float",
                    ["oscInputAddress"] = $"/avatar/parameters/{kv.Key}",
                    ["oscOutputAddress"] = $"/avatar/parameters/{kv.Key}",
                    ["inOsc"] = false,
                    ["inAvatar"] = true,
                    ["currentValue"] = kv.Value,
                    ["status"] = "avatar-only"
                });
            }

            int matchedCount = mergedParams.OfType<JObject>().Count(p => (string?)p["status"] == "matched");
            result["parameters"] = mergedParams;
            result["totalOsc"] = oscParams.Count;
            result["totalAvatar"] = avatarParamMap.Count;
            result["totalParams"] = mergedParams.Count;
            result["matched"] = matchedCount;
            result["mismatched"] = mergedParams.Count - matchedCount;

            AppLog.Success("OscExplorer", $"Loaded {mergedParams.Count} parameters ({matchedCount} matched) for {avatarId}");
            return result;
        }

        public static bool SendOscCommand(string address, string oscType, string rawValue)
        {
            try
            {
                byte[] packet = BuildOscPacket(address, oscType, rawValue);
                using var udp = new UdpClient();
                udp.Connect("127.0.0.1", 9000);
                udp.Send(packet, packet.Length);
                AppLog.Success("OSC", $"Sent → {address} ({oscType}) = {rawValue}");
                return true;
            }
            catch (Exception ex)
            {
                AppLog.Error("OSC", $"Send failed: {ex.Message}");
                return false;
            }
        }

        private static byte[] BuildOscPacket(string address, string oscType, string rawValue)
        {
            using var ms = new MemoryStream();
            WriteOscString(ms, address);

            switch (oscType.ToLowerInvariant())
            {
                case "float":
                {
                    WriteOscString(ms, ",f");
                    float fVal = float.TryParse(rawValue,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float f) ? f : 0f;
                    byte[] b = BitConverter.GetBytes(fVal);
                    if (BitConverter.IsLittleEndian) Array.Reverse(b);
                    ms.Write(b, 0, b.Length);
                    break;
                }
                case "int":
                {
                    WriteOscString(ms, ",i");
                    int iVal = int.TryParse(rawValue, out int iv) ? iv : 0;
                    byte[] b = BitConverter.GetBytes(iVal);
                    if (BitConverter.IsLittleEndian) Array.Reverse(b);
                    ms.Write(b, 0, b.Length);
                    break;
                }
                case "bool":
                {
                    bool bVal = rawValue == "1" || rawValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                    WriteOscString(ms, ",i");
                    int iVal = bVal ? 1 : 0;
                    byte[] b = BitConverter.GetBytes(iVal);
                    if (BitConverter.IsLittleEndian) Array.Reverse(b);
                    ms.Write(b, 0, b.Length);
                    break;
                }
                default:
                    goto case "float";
            }

            return ms.ToArray();
        }

        public static (bool success, string message) DeleteOscFile(string userId, string avatarId)
        {
            try
            {
                string oscPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "AppData", "LocalLow", "VRChat", "VRChat", "OSC", userId, "Avatars", avatarId + ".json");
                if (!File.Exists(oscPath))
                    return (false, "OSC config file not found.");
                File.Delete(oscPath);
                AppLog.Success("OscExplorer", $"Deleted OSC config: {oscPath}");
                return (true, "OSC config file deleted successfully.");
            }
            catch (Exception ex)
            {
                AppLog.Error("OscExplorer", $"Failed to delete OSC config: {ex.Message}");
                return (false, ex.Message);
            }
        }

        public static (bool success, string message) DeleteAvatarCache(string userId, string avatarId)
        {
            try
            {
                string aviPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "AppData", "LocalLow", "VRChat", "VRChat", "LocalAvatarData", userId, avatarId);
                if (!File.Exists(aviPath))
                    return (false, "Avatar cache file not found.");
                File.Delete(aviPath);
                AppLog.Success("OscExplorer", $"Deleted avatar cache: {aviPath}");
                return (true, "Avatar cache file deleted successfully.");
            }
            catch (Exception ex)
            {
                AppLog.Error("OscExplorer", $"Failed to delete avatar cache: {ex.Message}");
                return (false, ex.Message);
            }
        }

        private static void WriteOscString(MemoryStream ms, string s)
        {
            byte[] b = Encoding.ASCII.GetBytes(s + "\0");
            ms.Write(b, 0, b.Length);
            int pad = (4 - (b.Length % 4)) % 4;
            for (int i = 0; i < pad; i++) ms.WriteByte(0);
        }
    }
}
