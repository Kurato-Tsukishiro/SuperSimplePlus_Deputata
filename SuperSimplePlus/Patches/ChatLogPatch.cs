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
using static SuperSimplePlus.Patches.SaveChatLogPatch;

namespace SuperSimplePlus.Patches;

// 参考=>https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Patches/ChatHandlerPatch.cs
[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
class AddChatPatch
{
    /// <summary>
    /// チャットに流れた文字をチャットログを作成するメソッドに渡す。
    /// SNRのコマンドの返答等SNR側のSystemMessageの場合はSystemMessageとして反映する。
    /// </summary>
    /// <param name="sourcePlayer">チャット送信者</param>
    /// <param name="chatText">チャット内容</param>
    public static void Prefix(PlayerControl sourcePlayer, string chatText)
    {
        if (!sourcePlayer.name.Contains(SNRSystemMessage))
            SaveChatLog(GetChatLog(sourcePlayer, chatText));
        else
            SaveSystemLog(GetSystemMessageLog(chatText));
    }
}

internal static class SaveChatLogPatch
{
    internal static void Load()
    {
        ChatLogFileCreate();
        GameCount = 0;
    }

    /// <summary>
    /// ChatLogを出力するファイルのパス。
    /// ModLoad時に一回だけChatLogFileCreate()により作成している。
    /// </summary>
    private static string ChatLogFilePath;
    internal static int GameCount;
    /// <summary>
    /// SNRのシステムメッセージに含まれる文字列
    /// </summary>
    internal const string SNRSystemMessage
        = "<color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color>";

    /// <summary>
    /// Modロード時に出力先のパスを作成
    /// 参考=>https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Modules/Logger.cs
    /// 自分がSNRの方で作成したコードを参考として書く必要はあるのだろうか()
    /// </summary>
    private static void ChatLogFileCreate()
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
        SaveSystemLog(GetSystemMessageLog($"{string.Format(ModTranslation.getString("ChatLogFileCreate"), fileName)}"));
    }

    /// <summary>
    /// チャット内容をChatLogに記載する為に加工する。
    /// </summary>
    /// <param name="sourcePlayer">チャット送信者</param>
    /// <param name="chatText">チャット内容</param>
    /// <returns> chatLog : 加工した文字列</returns>
    internal static string GetChatLog(PlayerControl sourcePlayer, string chatText)
    {
        string chatLog = null;
        string date = DateTime.Now.ToString("HH:mm:ss");
        chatLog = $"[{date}] {sourcePlayer.name} ( {GetColorName(sourcePlayer.GetClient())} ) :「 {chatText} 」";

        return chatLog;
    }

    /// <summary>
    /// sシステムメッセージをChatLogに記載する為に加工する。
    /// </summary>
    /// <param name="systemMessageText">システムメッセージ</param>
    /// <returns></returns>
    internal static string GetSystemMessageLog(string systemMessageText)
    {
        string systemMessageLog = null;
        string date = DateTime.Now.ToString("HH:mm:ss");
        systemMessageLog = $"[{date}] SystemMessage  : 『 {systemMessageText} 』";

        return systemMessageLog;
    }

    /// <summary>
    /// チャットログをファイルに出力する
    /// 存在しないファイルに出力しようとした場合、エラーとしてLogOutput.logにチャットログを記載する。
    /// error対策を入れると正常に動かなくなった為、行っていない。必要なら方法を考える…
    /// </summary>
    /// <param name="chatLog"></param>
    internal static void SaveChatLog(string chatLog) => File.AppendAllText(ChatLogFilePath, $"{chatLog}" + Environment.NewLine);

    /// <summary>
    /// システムログをファイルに出力する
    /// </summary>
    /// <param name="systemMessageLog"></param>
    internal static void SaveSystemLog(string systemMessageLog) => File.AppendAllText(ChatLogFilePath, $"{systemMessageLog}" + Environment.NewLine);
}

/// <summary>
/// チャットログに記載する、システムメッセージに関わるHarmonyPatchを纏めている。
/// </summary>
[HarmonyPatch]
class ChatLogHarmonyPatch
{
    // ゲーム開始時に情報を記載する
    // 参考=> https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Patches/IntroPatch.cs
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin)), HarmonyPostfix]
    public static void IntroCutsceneCoBeginPostfix()
    {
        // TODO:確かサクランダーさんが「ログに試合数を記載したい」と言っていたので入れてみた。うまく動けばSNRにも実装したい
        GameCount++;
        SaveSystemLog(GetSystemMessageLog("|:===================================================================================:|"));

        SaveSystemLog(GetSystemMessageLog("=================Game Info================="));
        SaveSystemLog(GetSystemMessageLog($"{GameCount}回目の試合 開始"));
        SaveSystemLog(GetSystemMessageLog($"MapId:{GameManager.Instance.LogicOptions.currentGameOptions.MapId} MapNames:{(MapNames)GameManager.Instance.LogicOptions.currentGameOptions.MapId}"));

        SaveSystemLog(GetSystemMessageLog("=================Player Info================="));

        SaveSystemLog(GetSystemMessageLog("=================Player Data================="));
        SaveSystemLog(GetSystemMessageLog($"プレイヤー数：{PlayerControl.AllPlayerControls.Count}人"));
        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            SaveSystemLog(GetSystemMessageLog($"{p.name}(pid:{p.PlayerId})({p.GetClient()?.PlatformData?.Platform}"));

        SaveSystemLog(GetSystemMessageLog("|:===================================================================================:|"));
    }

    // 会議開始
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start)), HarmonyPostfix]
    public static void MeetingStartPostfix(MeetingHud __instance)
    {
        VariableManager.NumberOfMeetings++;
        SaveSystemLog(GetSystemMessageLog($"{GameCount}回目の試合の {VariableManager.NumberOfMeetings}回目の会議 開始"));
    }

    // 会議終了(airship以外)
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp)), HarmonyPostfix]
    public static void MeetingEndPostfix(ExileController __instance) => SystemLogMethodManager.DescribeMeetingEndSystemLog();

    // 会議終了(airship)
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn)), HarmonyPostfix]
    public static void AirshipMeetingEndPostfix(ExileController __instance) => SystemLogMethodManager.DescribeMeetingEndSystemLog();
}

/// <summary>
/// チャットログに記載する、システムメッセージに関わるメソッドを纏めている。
/// </summary>
internal static class SystemLogMethodManager
{
    internal static void DescribeMeetingEndSystemLog() =>
        SaveSystemLog(GetSystemMessageLog($"{GameCount}回目の試合の {VariableManager.NumberOfMeetings}回目の会議 終了"));
}
