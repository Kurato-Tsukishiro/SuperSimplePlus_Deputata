using System;
using System.Net.Http;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using SuperSimplePlus.Patches;
using InnerNet;
using AmongUs.Data;
using UnityEngine;

namespace SuperSimplePlus;
public static class ImmigrationCheck
{
    // 一番左と一行全部
    private static Dictionary<uint, string> dictionary = new(); //keyを行番号, valueをフレンドコードに
    internal static bool DenyEntryToFriendCode(ClientData client, string entryFriendCode)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        if (!SSPPlugin.FriendCodeBan.Value) return false;

        // 入室者のコードが辞書に乗っていたら BAN をする
        if (dictionary.ContainsValue(entryFriendCode))
        {
            AmongUsClient.Instance.KickPlayer(client.Id, ban: true);
            return true;
        }
        return false;
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
                        // 行ごとの文字列
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
                    catch { Logger.Error($"Error: Loading Translate.csv Line:{i}", "ModTranslation"); }
                }
            }
        }
        catch (Exception e) { Logger.Error($"[BANFriendCodeList.txt]のロードに失敗しました : {e}", "ImmigrationCheck"); }
    }
}
