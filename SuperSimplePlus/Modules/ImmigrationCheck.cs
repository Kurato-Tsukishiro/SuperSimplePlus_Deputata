using System;
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

    internal static void LoadFriendCodeList()
    {
        // ChatLogPatchの変数を使用せず 直接取得しているのは、こちらが読み込まれるのが早くて 正確にpathが取得できない事がある為
        string fileName = Path.GetDirectoryName(Application.dataPath) + @"\SSP_Deputata\" + @"BanFriendCodeList.txt";
        if (!File.Exists(fileName))
        {
            // [ ]MEMO:ファイル強制作成にする(できればファイルがないなら? => 説明書きありのファイルを作成が良い)
            FileStream fs = File.Create(fileName);
            fs.Close();
            Logger.Info("BANFriendCodeList.txtを作成しました。");
        }

        try
        {
            using (StreamReader sr = new(fileName))
            {
                uint i = 0;
                //1行ずつ処理
                while (!sr.EndOfStream)
                {
                    try
                    {
                        // 行ごとの文字列
                        string line = sr.ReadLine().Replace(" ", "");

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
