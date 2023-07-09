//参考=>https://github.com/Eisbison/TheOtherRoles/blob/main/TheOtherRoles/Modules/ModUpdater.cs
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using Twitch;
using UnityEngine;
using UnityEngine.UI;
using Version = SemanticVersioning.Version;

namespace SuperSimplePlus.Modules
{
    public class ModUpdater
    {
        public static string Tag;
        public static JObject data;
        public static GenericPopup popup;
        public static GameObject PopupButton;

        public async static Task<bool> IsNewer()//最新版かどうか判定
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "SuperSimplePlus Updater");

            var req = await client.GetAsync("https://api.github.com/repos/Kurato-Tsukishiro/SuperSimplePlus_Deputata/releases/latest", HttpCompletionOption.ResponseContentRead);
            if (!req.IsSuccessStatusCode) return false;

            var dataString = await req.Content.ReadAsStringAsync();
            data = JObject.Parse(dataString);

            Tag = data["tag_name"]?.ToString().TrimStart('v');
            Logger.Info($"最新版かどうか判定\ngithub:{Tag}\n入ってるバージョン:{SSPPlugin.Version}", "ModUpdater");

            return Version.TryParse(Tag, out var myVersion) && myVersion.BaseVersion() > Version.Parse(SSPPlugin.Version);
        }
        public async static Task<bool> DownloadUpdate()
        {
            try
            {
                PopupButton = popup.transform.GetChild(2).gameObject;
                PopupButton.SetActive(false);
                popup.TextAreaTMP.text = string.Format(ModTranslation.GetString("UpdateInProgress"), SSPPlugin.ColoredModName);

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "SuperSimplePlus Updater");

                JToken assets = data["assets"];
                string downloadURI = "";

                for (JToken current = assets.First; current != null; current = current.Next)
                {
                    string browser_download_url = current["browser_download_url"]?.ToString();
                    if (browser_download_url != null && current["content_type"] != null)
                    {
                        if (current["content_type"].ToString().Equals("application/x-msdownload") &&
                            browser_download_url.EndsWith(".dll"))
                        {
                            downloadURI = browser_download_url;
                        }
                    }
                }

                if (downloadURI.Length == 0) return false;

                var res = await client.GetAsync(downloadURI, HttpCompletionOption.ResponseContentRead);
                string filePath = Path.Combine(Paths.PluginPath, "SuperSimplePlus.dll");
                if (File.Exists(filePath + ".old")) File.Delete(filePath + ".old");
                if (File.Exists(filePath)) File.Move(filePath, filePath + ".old");

                await using var responseStream = await res.Content.ReadAsStreamAsync();
                await using var fileStream = File.Create(filePath);
                await responseStream.CopyToAsync(fileStream);

                PopupButton.SetActive(true);
            }
            catch (Exception e)
            {
                Logger.Error($"{e}", "ModUpdater");
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public class MainMenuStartPatch
        {
            public static void Postfix(MainMenuManager __instance)
            {
                string filePath = Path.Combine(Paths.PluginPath, "SuperSimplePlus.dll");//Oldファイルを削除する
                if (File.Exists(filePath + ".old")) File.Delete(filePath + ".old");

                Task<bool> SSPUpdateCheck = Task.Run(IsNewer);
                if (SSPUpdateCheck.Result) //最新版じゃなかったらボタンを作る
                {
                    popup = UnityEngine.Object.Instantiate(TwitchManager.Instance.TwitchPopup);
                    popup.TextAreaTMP.fontSize *= 0.7f;
                    popup.TextAreaTMP.enableAutoSizing = false;

                    var template = GameObject.Find("ExitGameButton");
                    if (template == null) return;

                    var buttonSSPUpdate = UnityEngine.Object.Instantiate(template, null);
                    buttonSSPUpdate.transform.localPosition = new Vector3(buttonSSPUpdate.transform.localPosition.x + 9.4f, buttonSSPUpdate.transform.localPosition.y + 4.2f, buttonSSPUpdate.transform.localPosition.z);

                    var textSSPUpdateButton = buttonSSPUpdate.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
                    __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
                    {
                        textSSPUpdateButton.SetText(String.Format(ModTranslation.GetString("UpdateButton"), SSPPlugin.ColoredModName));
                    })));

                    PassiveButton passiveButtonSSPUpdate = buttonSSPUpdate.GetComponent<PassiveButton>();

                    passiveButtonSSPUpdate.OnClick = new Button.ButtonClickedEvent();
                    passiveButtonSSPUpdate.OnClick.AddListener((System.Action)(() =>
                    {
                        popup.Show();
                        popup.TextAreaTMP.text =
                            Task.Run(DownloadUpdate).Result ?
                                String.Format(ModTranslation.GetString("UpdateSuccess"), SSPPlugin.ColoredModName) :
                                ModTranslation.GetString("UpdateFailed");
                        PopupButton.SetActive(true);
                    }));
                }
            }
        }
    }
}
