using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using UnityEngine;
using static System.String;
using static SuperSimplePlus.Helpers;

namespace SuperSimplePlus.Patches;

// 参考=>https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Patches/ChatHandlerPatch.cs
[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
class AddChatPatch
{
    /// <summary>
    /// チャットに流れた文字をチャットログを作成するメソッドに渡す
    /// </summary>
    /// <param name="sourcePlayer">チャット送信者</param>
    /// <param name="chatText">チャット内容</param>
    /// <returns>true:チャットをチャットに表記する / false:表記しない, 消す</returns>
    public static bool Prefix(PlayerControl sourcePlayer, string chatText)
    {
        SaveChatLogPatch.SaveLog(SaveChatLogPatch.GetChat(sourcePlayer, chatText));
        return true; // Chatは消さない!!
    }
}

internal static class SaveChatLogPatch
{
    /// <summary>
    /// ChatLogを出力するファイルのパス。
    /// ModLoad時に一回だけChatLogFileCreate()により作成している。
    /// </summary>
    private static string ChatLogFilePath;

    /// <summary>
    /// Modロード時に出力先のパスを作成
    /// 参考=>https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Modules/Logger.cs
    /// 自分がSNRの方で作成したコードを参考として書く必要はあるのだろうか()
    /// </summary>
    internal static void ChatLogFileCreate()
    {
        // ファイル名に使用する変数作成
        string date = DateTime.Now.ToString("yyMMdd_HHmm");

        // ファイル名作成
        string fileName = $"{date}_AmongUs_ChatLog.log";

        // 出力先のパス作成
        string folderPath = Path.GetDirectoryName(UnityEngine.Application.dataPath) + @"\SSP_Deputata\SaveChatLogFolder\";
        Directory.CreateDirectory(folderPath);
        ChatLogFilePath = @$"{folderPath}" + @$"{fileName}";

        Logger.Info($"{string.Format(ModTranslation.getString("ChatLogFileCreate"), fileName)}");
    }

    /// <summary>
    /// チャット内容をlogに記載する為加工する。
    /// </summary>
    /// <param name="sourcePlayer">チャット送信者</param>
    /// <param name="chatText">チャット内容</param>
    /// <returns> chatLog : 加工した文字列</returns>
    internal static string GetChat(PlayerControl sourcePlayer, string chatText)
    {
        string chatLog = null;
        string date = DateTime.Now.ToString("yy/MM/dd_HH:mm:ss");
        chatLog = $"[{date}] {sourcePlayer.name} ( {GetColorName(sourcePlayer.GetClient())} ) :「 {chatText} 」";

        return chatLog;
    }

    /// <summary>
    /// チャットログをファイルに出力する
    /// 存在しないファイルに出力しようとした場合、エラーとしてLogOutput.logにチャットログを記載する。
    /// error対策を入れると正常に動かなくなった為、行っていない。必要なら方法を考える…
    /// </summary>
    /// <param name="chatLog"></param>
    internal static void SaveLog(string chatLog) => File.AppendAllText(ChatLogFilePath, $"{chatLog}" + Environment.NewLine);
}
