//参考=>https://github.com/Eisbison/TheOtherRoles/blob/main/TheOtherRoles/Modules/ModUpdater.cs
using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using TMPro;
using Twitch;
using UnityEngine;
using UnityEngine.UI;
using Version = SemanticVersioning.Version;

namespace SuperSimplePlus.Modules
{
    public class ModUpdater
    {
        public static JObject data;
        public static GenericPopup popup;
        public static GameObject PopupButton;

        public async static Task<bool> IsNewer()//最新版かどうか判定
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "SuperSimplePlus Updater");

            var req = await client.GetAsync(new System.Uri($"https://api.github.com/repos/Kurato-Tsukishiro/SuperSimplePlus_Deputata/releases/latest"), HttpCompletionOption.ResponseContentRead);
            if (!req.IsSuccessStatusCode) return false;

            var dataString = await req.Content.ReadAsStringAsync();
            data = JObject.Parse(dataString);

            var GitHubTag = Regex.Replace(data["tag_name"]?.ToString(), "[^0-9.]", "");
            var LocalVersionStr = Regex.Replace(SSPPlugin.Version, "[^0-9.]", "");

            if (int.TryParse(GitHubTag.Replace(".", ""), out int githubVersion) && int.TryParse(LocalVersionStr.Replace(".", ""), out var localVersion))
            {
                Logger.Info($"最新版かどうか判定", "ModUpdater");
                Logger.Info($"GitHub Version: {GitHubTag}", "ModUpdater");
                Logger.Info($"Local Version: {LocalVersionStr}", "ModUpdater");

                var Islatest = githubVersion <= localVersion;
                if (Islatest) Logger.Info($"最新版です。", "ModUpdater");
                else Logger.Info($"古いバージョンを使用しています。", "ModUpdater");

                return !Islatest;
            }
            else
            {
                Logger.Error($"バージョン文字列の解析に失敗しました。 GitHub: '{GitHubTag}', Local: '{SSPPlugin.Version}'");
                return false;
            }

            // FIXME : TryParse() がどうしても失敗してしまう (Geminiに聞いたが, Geminiも正しく動くはずと言い出している) 為, SemanticVersioning.Version での判定を諦めた。
            /*
            // TryParseが成功したかどうかをif文で判定する
            if (Version.TryParse(cleanedGitHubTag, out var githubVersion) && Version.TryParse(cleanedLocalVersion, out var localVersion))
            {
                // パースに成功した場合のみ、バージョンを比較する
                Logger.Info($"GitHub Version: {githubVersion}");
                Logger.Info($"Local Version: {localVersion}");

                // Versionオブジェクトは > 演算子で直接比較できる
                return githubVersion > localVersion;
            }
            else
            {
                // パースに失敗した場合は、更新なし（またはエラー）として扱う
                Logger.Error($"バージョン文字列の解析に失敗しました。 GitHub: '{cleanedGitHubTag}', Local: '{SSPPlugin.Version}'");
                return false;
            }*/
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
                        if (current["content_type"].ToString().Equals("application/octet-stream") || current["content_type"].ToString().Equals("application/x-msdownload") &&
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
                    GameObject.Destroy(buttonSSPUpdate.GetComponent<AspectPosition>());
                    buttonSSPUpdate.transform.localPosition = new(-0.9f, -2.45f, -10f);

                    var textSSPUpdateButton = buttonSSPUpdate.GetComponentInChildren<TextMeshPro>();
                    textSSPUpdateButton.transform.localPosition = new(-0.5f, 0.035f, -2f);
                    textSSPUpdateButton.alignment = TextAlignmentOptions.Center;
                    __instance.StartCoroutine(Effects.Lerp(1f, new System.Action<float>((p) =>
                    {
                        textSSPUpdateButton.text = (String.Format(ModTranslation.GetString("UpdateButton"), SSPPlugin.ColoredModName));
                    })));

                    PassiveButton passiveButtonSSPUpdate = buttonSSPUpdate.GetComponent<PassiveButton>();

                    passiveButtonSSPUpdate.OnClick = new Button.ButtonClickedEvent();
                    passiveButtonSSPUpdate.OnClick.AddListener((System.Action)(() =>
                    {
                        popup.Show();

                        popup.TextAreaTMP.text =
                            File.Exists(filePath + ".old")
                                ? ModTranslation.GetString("UpdateAlready")
                                : Task.Run(DownloadUpdate).Result
                                    ? string.Format(ModTranslation.GetString("UpdateSuccess"), SSPPlugin.ColoredModName)
                                    : ModTranslation.GetString("UpdateFailed");

                        PopupButton.SetActive(true);
                    }));
                }
            }
        }
    }
}
