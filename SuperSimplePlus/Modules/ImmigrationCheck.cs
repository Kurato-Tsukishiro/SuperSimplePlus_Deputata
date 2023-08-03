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

namespace SuperSimplePlus;
public static class ImmigrationCheck
{
    // 一番左と一行全部
    private static Dictionary<uint, string> dictionary = new(); //keyを行番号, valueをフレンドコードに

    /// <summary>
    /// BANListの照会を行う。ranがtrueの時対象者のBANも実行する。
    /// </summary>
    /// <param name="client">照会対象</param>
    /// <param name="ran">BANを実行する。</param>
    /// <returns>>true / 対象者である, false / 対象者でない</returns>
    internal static bool DenyEntryToFriendCode(ClientData client, bool ran = false)
    {
        var result = dictionary.ContainsValue(client?.FriendCode);

        if (ran && result)
        {
            if (AmongUsClient.Instance.AmHost && SSPPlugin.FriendCodeBan.Value)
                AmongUsClient.Instance.KickPlayer(client.Id, ban: true); // 入室者のコードが辞書に乗っていたら BAN をする
        }

        return result;
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
}
