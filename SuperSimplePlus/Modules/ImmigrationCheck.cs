using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using AmongUs.Data;
using InnerNet;
using SuperSimplePlus.Patches;
using UnityEngine;

namespace SuperSimplePlus.Modules;
internal static class ImmigrationCheck
{
    // 一番左と一行全部
    private static readonly Dictionary<uint, string> dictionary = new(); //keyを行番号, valueをフレンドコードに

    /// <summary>BANListの照会を行う</summary>
    /// <param name="client">照会対象</param>
    /// <returns>true / 対象者である, false / 対象者でない</returns>
    internal static bool DenyEntryToFriendCode(ClientData client)
    {
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame) return false;
        if (client == null)
        {
            Logger.Error($"照会対象が存在しません。");
            return false;
        }

        var isSavedFriendCode = dictionary.ContainsValue(client.FriendCode);

        bool isTaregt;
        isTaregt = !HasFriendCode(client) || isSavedFriendCode;

        return isTaregt;
    }

    internal static async void LoadFriendCodeList()
    {
        // ChatLogPatchの変数を使用せず 直接取得しているのは、こちらが読み込まれるのが早くて 正確にpathが取得できない事がある為
        string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SSP_Deputata\" + @"BanFriendCodeList.txt";

        // [ BanFriendCodeList.txt ] が存在しない場合は, デフォルトの文章をmainブランチから取得し作成する。
        if (!File.Exists(filePath))
        {
            const string remoteUri = "https://raw.githubusercontent.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/main/SuperSimplePlus/Resources/BanFriendCodeList.txt";
            const string fileName = "BanFriendCodeList.txt";
            HttpClient client = new();

            try
            {
                using (HttpResponseMessage response = await client.GetAsync(remoteUri))
                {
                    response.EnsureSuccessStatusCode();

                    using var pullStream = await response.Content.ReadAsStreamAsync();
                    response.EnsureSuccessStatusCode();

                    using var outStream = File.Create(filePath);
                    using var writeStream = await response.Content.ReadAsStreamAsync();
                    writeStream.CopyTo(outStream);
                    response.EnsureSuccessStatusCode();

                    Logger.Info($"{fileName}を作成しました。");
                }
            }
            catch (Exception e) { Logger.Error($"{fileName}の作成に失敗しました。 : {e}"); }
        }

        try
        {
            using (StreamReader sr = new(filePath))
            {
                uint i = 0;
                //1行ずつ処理
                while (!sr.EndOfStream)
                {
                    try
                    {
                        // 行ごとの文字列 ' 'と'　'は 消去する。
                        string line = sr.ReadLine().Replace(" ", "").Replace("　", "");

                        // 行が空白 戦闘が#なら次の行に
                        if (line == "" || line[0] == '#') continue;

                        // "//"以下をコメント文とみなし 取得しない
                        var length = line.IndexOf("//");
                        string FriendCode = length < 0 ? line : line[..length];

                        // 辞書に格納する
                        dictionary.Add(i, FriendCode);
                        i++;
                    }
                    catch (Exception e) { Logger.Error($"{i} 個目の取得に失敗しました。 : {e}", "ImmigrationCheck"); }
                }
                Logger.Info($"{i + 1} 個のFriendCodeが 登録されています。");
            }
        }
        catch (Exception e) { Logger.Error($"[BANFriendCodeList.txt]のロードに失敗しました : {e}", "ImmigrationCheck"); }
    }

    /// <summary>手動でBAN又はKickを行った場合、BenReport.logに記録する</summary>
    /// <param name="client">対象</param>
    /// <param name="reason">切断理由</param>
    internal static void WriteBanReport(ClientData client, DisconnectReasons reason)
    {
        if (reason is not DisconnectReasons.Banned and not DisconnectReasons.Kicked) return;

        if (!AmongUsClient.Instance.AmHost) return;
        if (DenyEntryToFriendCode(client)) return; // 既にBunListに登録されている場合は記載しない。

        // PC以外BANが有効で, Steam・Epic でない場合, 自動BANなので記載しない。
        if (SSPPlugin.NotPCBan.Value && (client.PlatformData.Platform is not Platforms.StandaloneEpicPC and not Platforms.StandaloneSteamPC)) return;
        string bunReportPath = @$"{GameLogManager.SSPDFolderPath}" + @$"BenReport.log";

        Logger.Info($"Listに登録していない人の手動BAN 又は手動キックを行った為, 保存します。 =>({reason}) {client.PlayerName} : {FriendCodeFormatString(client)}");
        string log = $"登録日時 : {DateTime.Now:yyMMdd_HHmm}, 登録者 : {client.PlayerName} ( {client.FriendCode} ), 理由 : {reason}, プラットフォーム : {client.PlatformData.Platform}";
        File.AppendAllText(bunReportPath, log + Environment.NewLine);
    }

    // 参考 => https://github.com/SuperNewRoles/SuperNewRoles/blob/2.1.1.1/SuperNewRoles/Modules/Blacklist.cs#L109-L113
    /// <summary>フレンドコードを有するか</summary>
    /// <param name="client">確認対象</param>
    /// <returns>true = 有する / false = 有さない</returns>
    internal static bool HasFriendCode(ClientData client)
        => client != null && (client.FriendCode is not null and not "") && client.FriendCode.Contains('#');

    /// <summary>フレンドコードをログに記載する形に変換する</summary>
    /// <param name="client">取得したい対象</param>
    /// <returns>変換されたフレンドコード</returns>
    internal static string FriendCodeFormatString(ClientData client)
        => client == null ? "Error" : !HasFriendCode(client) ? "未所持" : SSPPlugin.HideFriendCode.Value ? "**********#****" : client.FriendCode;
}
