using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InnerNet;
using UnityEngine;
using static System.String;
using static SuperSimplePlus.Helpers;
using static SuperSimplePlus.Patches.SaveChatLogPatch;

namespace SuperSimplePlus.Patches;
internal static class GameLogManager
{
    internal static void Load()
    {
        GameLogFileCreate();
    }

    internal static string SSPDFolderPath { get { return _SSPDFolderPath; } }
    private static string _SSPDFolderPath;

    /// <summary>
    /// ChatLogを出力するファイルのパス 読み取り専用
    /// 書き込みはModLoad時に一度のみChatLogFileCreate()で行い、_chatLogFilePathに保存している。
    /// </summary>
    internal static string ChatLogFilePath { get { return _chatLogFilePath; } }
    private static string _chatLogFilePath;
    internal static string RoundGameLogFilePath { get { return _roundGameLogFilePath; } }
    private static string _roundGameLogFilePath;

    internal static StringBuilder NowGameLog = new();
    internal static readonly Dictionary<int, string> GameLogDic = new();

    internal static int GameCount = 0;

    /// <summary>
    /// Modロード時に出力先のパスを作成
    /// 参考=>https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Modules/Logger.cs
    /// 自分がSNRの方で作成したコードを参考として書く必要はあるのだろうか()
    /// </summary>
    private static void GameLogFileCreate()
    {
        // 出力先のパス作成
        _SSPDFolderPath = Path.GetDirectoryName(Application.dataPath) + @"\SSP_Deputata\";
        Directory.CreateDirectory(_SSPDFolderPath);

        string chatLogFolderPath = @$"{_SSPDFolderPath}\SaveChatAllLogFolder\";
        _roundGameLogFilePath = @$"{_SSPDFolderPath}\RoundGameLogFolder\";
        Directory.CreateDirectory(chatLogFolderPath);
        Directory.CreateDirectory(RoundGameLogFilePath);

        // ファイル名に使用する変数作成
        string date = DateTime.Now.ToString("yyMMdd_HHmm");

        // ファイル名作成
        string fileName = $"{date}_AmongUs_GameLog.log";
        _chatLogFilePath = @$"{chatLogFolderPath}" + @$"{fileName}";

        if (!SSPPlugin.ChatLog.Value) return;

        Logger.Info($"{Format(ModTranslation.GetString("ChatLogFileCreate"), fileName)}");
        SaveSystemLog(GetSystemMessageLog($"{Format(ModTranslation.GetString("ChatLogFileCreate"), fileName)}"));
    }

    internal static string ManuallySaveGameLog(string importFileName, int count)
    {
        GemeLogSaveAs(ReplaceUnusableStringsAsFileNames(importFileName), count, out string saveStatusMessage); // ゲームログを取得する。
        return saveStatusMessage;
    }

    internal static async void AddGameLog()
    {
        string useLogString = NowGameLog.ToString();
        NowGameLog = new();

        if (GameLogDic.ContainsKey(GameCount)) return;

        GameLogDic.Add(GameCount, useLogString);
        using StreamWriter sw = new(ChatLogFilePath, true);
        await sw.WriteLineAsync(useLogString);
    }

    private static (string log, bool success) GetGameLogDic(int count) =>
        count <= 0 ? (Format(ModTranslation.GetString("GetGameLogDicError"), count), false) : GameLogDic.ContainsKey(count) ? (GameLogDic[count], true) : (Format(ModTranslation.GetString("GetGameLogDicError"), count), false);


    /// <summary>
    /// 一個前の試合のGameLogのみを任意のファイル名で保存する。
    /// getCountを省略した(引数として`0`を渡した)場合、「最終のログ」を取得する
    /// </summary>
    /// <param name="name">任意のfile名</param>
    /// <param name="getCount">ログを取得したいゲームの回数</param>
    /// <returns>string : 保存処理の結果</returns>
    private static void GemeLogSaveAs(string name, int getCount, out string saveStatusMessage)
    {
        string date = DateTime.Now.ToString("yyMMdd_HHmm");
        string fileName = $"{date}_{getCount}_{name}_GameLog" + ".log";
        string newFilePath = @$"{RoundGameLogFilePath}/{fileName}";

        try
        {
            string log;
            bool success;
            (log, success) = GetGameLogDic(getCount);

            if (success)
            {
                using StreamWriter sw = new(newFilePath, false);
                sw.WriteLine(log);
                saveStatusMessage = $"[ {fileName} ] に ゲームログを抜き出しました。";
            }
            else
            {
                Logger.Error(log);
                saveStatusMessage = $"[Error] {log}";
            }
        }
        catch (Exception e)
        {
            Logger.Error($"ゲームログの抜き出し保存に失敗しました : {e}");
            saveStatusMessage = "[Error] ゲームログの抜き出し保存に失敗しました。";
        }
    }
}
