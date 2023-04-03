using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using InnerNet;
using UnityEngine;
using static System.String;
using static SuperSimplePlus.Helpers;
using static SuperSimplePlus.Patches.SaveChatLogPatch;
using static SuperSimplePlus.Patches.SystemLogMethodManager;

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
        if (!sourcePlayer.GetClient().PlayerName.Contains(SNRSystemMessage))
            SaveChatLog(GetChatLog(sourcePlayer.GetClient(), chatText));
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
    internal static string GetChatLog(ClientData sourceClient, string chatText)
    {
        string chatLog = null;
        string date = DateTime.Now.ToString("HH:mm:ss");
        chatLog = $"[{date}] {sourceClient.PlayerName} ( {GetColorName(sourceClient)} ) :「 {chatText} 」";
        chatLog = !sourceClient.GetPlayer().IsDead()
            ? $"[{date}]        {sourceClient.PlayerName} :「 {chatText} 」"
            : $"[{date}] (死者) {sourceClient.PlayerName} :「 {chatText} 」";

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
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin)), HarmonyPostfix]
    public static void IntroCutsceneCoBeginPostfix() => IntroCutsceneCoBeginSystemLog();

    // 会議開始
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start)), HarmonyPostfix]
    public static void MeetingStartPostfix(MeetingHud __instance) => MeetingStartSystemLog(__instance);

    // 死体通報(Hostのみ)
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody)), HarmonyPrefix]
    public static void ReportDeadBodyPrefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target) => ReportDeadBodySystemLog(__instance, target);

    // 投票感知&記載(Hostのみ)
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote)), HarmonyPostfix]
    public static void MeetingCastVotePostfix(byte srcPlayerId, byte suspectPlayerId) => MeetingCastVoteSystemLog(srcPlayerId, suspectPlayerId);

    // 開票(Hostのみ)
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting)), HarmonyPostfix]
    public static void CheckForEndVotingPrefix(MeetingHud __instance) => MeetingCastVoteSave(__instance);

    // 会議終了(airship以外)
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp)), HarmonyPostfix]
    public static void MeetingEndPostfix(ExileController __instance) => DescribeMeetingEndSystemLog(__instance.exiled);

    // 会議終了(airship)
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn)), HarmonyPostfix]
    public static void AirshipMeetingEndPostfix(ExileController __instance) => DescribeMeetingEndSystemLog(__instance.exiled);

    // キル発生時
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer)), HarmonyPostfix]
    public static void MurderPlayerPostfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target) => MurderPlayerSystemLog(__instance, target);

    // 試合終了
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp)), HarmonyPostfix]
    public static void EndGamePostfix() => EndGameSystemLog();

}

/// <summary>
/// チャットログに記載する、システムメッセージに関わるメソッドを纏めている。
/// </summary>
internal static class SystemLogMethodManager
{
    private const string delimiterLine
    = "|:===================================================================================:|";

    // ゲーム開始時

    /// <summary>
    /// 参考=> https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Patches/IntroPatch.cs
    /// 参考元と違い[PlayerControl p] ではなく [ClientData client] を用いているのは、
    /// 此方を使用しないとSNRでSHR使用時暗転させる為。SHRとの競合回避用。
    /// </summary>
    internal static void IntroCutsceneCoBeginSystemLog()
    {
        // TODO:確かサクランダーさんが「ログに試合数を記載したい」と言っていたので入れてみた。うまく動けばSNRにも実装したい
        GameCount++;
        SaveSystemLog(GetSystemMessageLog(delimiterLine));

        SaveSystemLog(GetSystemMessageLog("=================Game Info================="));
        SaveSystemLog(GetSystemMessageLog($"{GameCount}回目の試合 開始"));
        SaveSystemLog(GetSystemMessageLog($"MapId:{GameManager.Instance.LogicOptions.currentGameOptions.MapId} MapNames:{(MapNames)GameManager.Instance.LogicOptions.currentGameOptions.MapId}"));

        SaveSystemLog(GetSystemMessageLog("=================Player Info================="));

        SaveSystemLog(GetSystemMessageLog("=================Player Data================="));
        SaveSystemLog(GetSystemMessageLog($"プレイヤー数：{PlayerControl.AllPlayerControls.Count}人"));
        foreach (ClientData client in AmongUsClient.Instance.allClients)
            SaveSystemLog(GetSystemMessageLog($"{client.PlayerName}(cid:{client.Id})(pid:{client.GetPlayer().PlayerId})({GetColorName(client)})({client?.PlatformData?.Platform})"));

        SaveSystemLog(GetSystemMessageLog(delimiterLine));
    }

    // 会議開始
    internal static void MeetingStartSystemLog(MeetingHud __instance)
    {
        VariableManager.NumberOfMeetings++;
        SaveSystemLog(GetSystemMessageLog("=================Task Phase End=================\n"));

        SaveSystemLog(GetSystemMessageLog("=================Meeting Phase Start================="));

        SaveSystemLog(GetSystemMessageLog("=================Start Meeting Info================="));

        SaveSystemLog(GetSystemMessageLog($"{GameCount}回目の試合の {VariableManager.NumberOfMeetings}回目の会議 開始"));

        SaveSystemLog(GetSystemMessageLog("=================Time of the crime and the killers and victims Info Start================="));

        SaveSystemLog(GetSystemMessageLog($"{VariableManager.NumberOfMeetings}ターン目, タスクフェイズ中の 犯行時刻及び 殺害者と犠牲者"));
        CrimeTimeAndKillerAndVictimLog();
        VariableManager.CrimeTimeAndKillersAndVictims = new();

        SaveSystemLog(GetSystemMessageLog("=================Time of the crime and the killers and victims Info End================="));

        SaveSystemLog(GetSystemMessageLog("===================================================="));
    }

    // 死体通報
    internal static void ReportDeadBodySystemLog(PlayerControl convener, GameData.PlayerInfo target)
    {
        Logger.Info("ゲスト生きてる?");
        if (convener == null) return;
        if (target == null) SaveSystemLog(GetSystemMessageLog($"[{convener.name}] が 緊急招集しました。"));
        else SaveSystemLog(GetSystemMessageLog($"[{convener.name}] が [{target.Object.name}] の死体を通報しました。"));
    }

    /// <summary>
    /// 投票時にチャットログに投票内容を記載する。
    /// </summary>
    /// <param name="srcPlayerId">投票者のPlayerId</param>
    /// <param name="suspectPlayerId">投票先のPlayerId</param>
    internal static void MeetingCastVoteSystemLog(byte srcPlayerId, byte suspectPlayerId) => OpenVoteSystemLog(srcPlayerId, suspectPlayerId);

    /// <summary>
    /// 投票状況を辞書に格納する。
    /// ClientIdを保存していない理由は, スキップや無投票等がPlayerIdを流用している為、正確な情報を保存できなくなるから。
    /// 参考=>https://github.com/yukieiji/ExtremeRoles/blob/55b1bb54557cf036de2ec7d64d709dde673e17ec/ExtremeRoles/Patches/Meeting/MeetingHudPatch.cs#L277-L293
    /// </summary>
    /// <param name="__instance"></param>
    internal static void MeetingCastVoteSave(MeetingHud __instance)
    {
        foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
        {
            byte srcPlayerId = playerVoteArea.TargetPlayerId;
            byte suspectPlayerId = playerVoteArea.VotedFor;

            // 投票先を全格納
            if (VariableManager.ResultsOfTheVoteCount.ContainsKey(srcPlayerId)) // key重複対策
                VariableManager.ResultsOfTheVoteCount[srcPlayerId] = suspectPlayerId; // key重複時は投票先を上書きする
            else VariableManager.ResultsOfTheVoteCount.Add(srcPlayerId, suspectPlayerId);
        }
    }

    /// <summary>
    /// 会議終了時に最終的な投票結果を纏めて記載する為に、辞書を開き投票結果を記載するメソッドに渡す。
    /// 投票結果を保存している辞書を初期化する。
    /// </summary>
    internal static void OpenVoteDecoding()
    {
        SaveSystemLog(GetSystemMessageLog("=================Open Votes Info Start================="));

        foreach (KeyValuePair<byte, byte> kvp in VariableManager.ResultsOfTheVoteCount)
            OpenVoteSystemLog(kvp.Key, kvp.Value);

        SaveSystemLog(GetSystemMessageLog("=================Open Votes Info End================="));

        VariableManager.ResultsOfTheVoteCount = new();
    }

    /// <summary>
    /// 投票結果を記載する。
    /// [suspectPlayerId]にスキップ等の情報が、PlayerIdの流用により乗せられている為、CDを渡す事ができず、このメソッド内でCDを取得している。
    /// </summary>
    /// <param name="srcPlayerId">投票者のPlayerId</param>
    /// <param name="suspectPlayerId">投票先のPlayerId</param>
    internal static void OpenVoteSystemLog(byte srcPlayerId, byte suspectPlayerId)
    {
        PlayerControl srcPC = PlayerById(srcPlayerId);
        if (srcPC.IsDead()) return; // なんで[MeetingHud.CheckForEndVoting]は死者の投票状態まで送られるねん() 霊界から投票できるのかよ()()()

        string srcName = srcPC.GetClient().PlayerName;
        string suspectName = "";

        string OpenVoteMessage = $"[{srcName}] が [{suspectName}] に投票しました。";

        switch (suspectPlayerId)
        {
            case 252:
                suspectName = "???";
                break;
            case 253:
                suspectName = "スキップ";
                break;
            case 254:
                suspectName = "無投票";
                OpenVoteMessage = $"[{srcName}] は [{suspectName}] でした。";
                break;
            case 255:
                suspectName = "未投票";
                OpenVoteMessage = $"[{srcName}] は [{suspectName}] です。";
                break;
            default:
                suspectName = PlayerById(suspectPlayerId).GetClient().PlayerName;
                break;
        }

        SaveSystemLog(GetSystemMessageLog($"{OpenVoteMessage}"));
    }

    // 会議終了
    internal static void DescribeMeetingEndSystemLog(GameData.PlayerInfo exiled)
    {
        SaveSystemLog(GetSystemMessageLog("=================End Meeting Info================="));
        if (exiled != null && exiled.Object == null) exiled = null;
        SaveSystemLog(GetSystemMessageLog($"{GameCount}回目の試合の {VariableManager.NumberOfMeetings}回目の会議 終了"));
        if (exiled == null) SaveSystemLog(GetSystemMessageLog($"誰も追放されませんでした。"));
        else SaveSystemLog(GetSystemMessageLog($"[ {exiled.Object.name} ] が追放されました。"));

        // 投票情報記載
        OpenVoteDecoding();

        SaveSystemLog(GetSystemMessageLog("=================Time of the crime and the killers and victims Info Start================="));

        SaveSystemLog(GetSystemMessageLog($"{VariableManager.NumberOfMeetings}ターン目, ミーティングフェイズ中の 犯行時刻及び 殺害者と犠牲者"));
        CrimeTimeAndKillerAndVictimLog();
        VariableManager.CrimeTimeAndKillersAndVictims = new();

        SaveSystemLog(GetSystemMessageLog("=================Time of the crime and the killers and victims Info End================="));

        SaveSystemLog(GetSystemMessageLog("===================================================="));
        SaveSystemLog(GetSystemMessageLog("=================Meeting Phase End=================\n"));
        SaveSystemLog(GetSystemMessageLog("=================Task Phase Start================="));
    }


    internal static void CrimeTimeAndKillerAndVictimLog()
    {
        foreach (KeyValuePair<DateTime, (ClientData, ClientData)> kvp in VariableManager.CrimeTimeAndKillersAndVictims)
        {
            ClientData killerClient = kvp.Value.Item1;
            ClientData victimClient = kvp.Value.Item2;

            if (kvp.Key == null && killerClient == null && victimClient == null) continue;

            string crimeTime = kvp.Key != null ? kvp.Key.ToString("HH:mm:ss") : "死亡時刻不明";
            string killerName = killerClient.PlayerName ?? "不明";
            string victimName = victimClient.PlayerName ?? "身元不明";
            string victimColor = victimClient != null ? GetColorName(victimClient) : "";

            SaveSystemLog(GetSystemMessageLog($"犯行時刻:[{crimeTime}] 殺害者:[{killerName}] 犠牲者:[{victimName} ({victimColor})]"));
        }
    }

    // キル発生時
    internal static void MurderPlayerSystemLog(PlayerControl Killer, PlayerControl victim)
    {
        SaveSystemLog(GetSystemMessageLog($"[ {Killer.name} ] が [ {victim.name} ]を殺害しました。"));
        VariableManager.CrimeTimeAndKillersAndVictims[DateTime.Now] = (Killer.GetClient(), victim.GetClient());
    }

    // 試合終了
    internal static void EndGameSystemLog()
    {
        SaveSystemLog(GetSystemMessageLog(delimiterLine));
        SaveSystemLog(GetSystemMessageLog("=================End Game Info================="));
        SaveSystemLog(GetSystemMessageLog($"{GameCount}回目の試合 終了"));
        SaveSystemLog(GetSystemMessageLog(delimiterLine));
    }
}
