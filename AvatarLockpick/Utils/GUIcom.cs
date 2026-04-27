using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AvatarLockpick.Utils
{
    internal class GUIcom
    {
        public static async void Communication(string s, Photino.NET.PhotinoWindow window = null)
        {
            try
            {
                //Json Tokens
                JObject jsonData = JObject.Parse(s);
                JToken actionToken = jsonData["action"];
                //JToken AviInfoActionToken = jsonData["avatarInfo"];
                JToken AviIDToken = jsonData["avatarId"];
                JToken UserIDToken = jsonData["userId"];
                //JToken HelpUrlToken = jsonData["openHelpUrl"];
                JToken TypeToken = jsonData["type"];
                JToken HistoryDataToken = jsonData["payload"];

                if ((string)jsonData["type"] == "avatarInfo")
                {
                    // Deprecated: UI now triggers modal and fetchAvatarInfo.
                    // Kept for fallback if needed.
                }

                if ((string)jsonData["type"] == "openInBrowser")
                {
                    var avatarInfo = new
                    {
                        AvatarId = (string)jsonData["avatarId"],
                        UserId = (string)jsonData["userId"]
                    };
                    URLStuff.OpenUrl($"https://vrchat.com/home/avatar/{avatarInfo.AvatarId}");
                }

                if ((string)jsonData["type"] == "checkPawStatus")
                {
                    Task.Run(async () => {
                        await PAWUtils.CheckApiStatus();
                        if (window != null)
                        {
                            var statusData = new { type = "pawStatus", isOnline = PAWUtils.IsApiOnline };
                            window.SendWebMessage(Newtonsoft.Json.JsonConvert.SerializeObject(statusData));
                        }
                    });
                }

                if ((string)jsonData["type"] == "fetchAvatarInfo")
                {
                    string aviId = AviIDToken?.ToString();
                    Task.Run(async () => {
                        var avatar = await PAWUtils.GetAvatarAsync(aviId);
                        if (window != null)
                        {
                            var resp = new
                            {
                                type = "pawAvatarInfo",
                                avatarId = aviId,
                                found = avatar != null,
                                name = avatar?.Name,
                                authorName = avatar?.AuthorName,
                                description = avatar?.Description,
                                imageUrl = avatar?.ImageUrl ?? avatar?.ThumbnailUrl
                            };
                            window.SendWebMessage(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                        }
                    });
                }

                if ((string)jsonData["type"] == "openHelpUrl")
                {
                    URLStuff.OpenUrl($"https://github.com/scrim-dev/AvatarLockpick/blob/master/HELP.md");
                }

                if ((string)jsonData["type"] == "restart-novr")
                {
                    Task.Run(() =>
                    {
                        VRC.CloseVRChat();
                        Task.Delay(1000); //Just in case
                        VRC.LaunchVRChat(false);
                    });
                }

                if ((string)jsonData["type"] == "restart-vr")
                {
                    Task.Run(() =>
                    {
                        VRC.CloseVRChat();
                        Task.Delay(1000); //Just in case
                        VRC.LaunchVRChat(true);
                    });
                }

                if((string)jsonData["type"] == "exportHistory")
                {
                    File.WriteAllText($"UI/ALP_History.json", HistoryDataToken.ToString());
                    AppLog.Success("GUIcom", "History data exported successfully to UI/ALP_History.json");
                }

                if ((string)jsonData["type"] == "customDbSettings")
                {
                    string customUrl = (string)jsonData["url"] ?? "";
                    string customPath = (string)jsonData["path"] ?? "";
                    AvatarUnlocker.SetCustomDatabase(customUrl, customPath);
                    AppLog.Log("GUIcom", $"Custom database settings updated - URL: {(string.IsNullOrEmpty(customUrl) ? "(default)" : customUrl)}, Path: {(string.IsNullOrEmpty(customPath) ? "(default)" : customPath)}");
                }

                if ((string)jsonData["type"] == "linuxSettings")
                {
                    string linuxPath = (string)jsonData["path"] ?? "";
                    AvatarUnlocker.SetLinuxCachePath(linuxPath);
                    AppLog.Log("GUIcom", $"Linux custom cache path updated: {(string.IsNullOrEmpty(linuxPath) ? "(default)" : linuxPath)}");
                }

                if (actionToken != null && actionToken.Type == JTokenType.String)
                {
                    string action = actionToken.ToString();
                    switch (action)
                    {
                        case "Unlock":
                            MessageBoxUtils.Show("Press OK to unlock the avatar.", "Unlock Avatar", 0x00000000U);
                            AvatarUnlocker.Start(1, UserIDToken.ToString(), AviIDToken.ToString());
                            break;
                        case "Unlock All":
                            MessageBoxUtils.Show("Press OK to unlock all avatars", "Unlock All Avatars", 0x00000000U);
                            AvatarUnlocker.Start(2, UserIDToken.ToString(), AviIDToken.ToString());
                            break;
                        case "Unlock (VRCFury)":
                            MessageBoxUtils.Show("Press OK to unlock VRCFury locked avatar", "Unlock VRCFury", 0x00000000U);
                            AvatarUnlocker.Start(3, UserIDToken.ToString(), AviIDToken.ToString());
                            break;
                        case "Database Unlock":
                            MessageBoxUtils.Show("Press OK to try unlocking the lock using the SQL database", "Unlock via SQL DB", 0x00000000U);
                            AvatarUnlocker.Start(4, UserIDToken.ToString(), AviIDToken.ToString());
                            break;
                        case "openAppDirectory":
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                            {
                                FileName = AppDomain.CurrentDomain.BaseDirectory,
                                UseShellExecute = true
                            });
                            break;
                        default:
                            // Handle unknown action
                            MessageBoxUtils.ShowInfo($"Unknown action: {action}");
                            break;
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                // Handle invalid JSON format
                MessageBoxUtils.ShowError($"Error parsing JSON with Newtonsoft.Json: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other potential errors
                MessageBoxUtils.ShowError($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
