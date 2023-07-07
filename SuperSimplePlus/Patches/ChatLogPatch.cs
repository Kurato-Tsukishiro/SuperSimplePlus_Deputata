using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using InnerNet;
using UnityEngine;
using static System.String;
using static SuperSimplePlus.Helpers;
using static SuperSimplePlus.Patches.SaveChatLogPatch;
using static SuperSimplePlus.Patches.SystemLogMethodManager;
using static SuperSimplePlus.Patches.SystemLogMethodManager.VoteLogMethodManager;

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
        if (!SSPPlugin.ChatLog.Value) return; // ChatLogを作成しない設定だったら読まないようにする。

        if (!sourcePlayer.GetClient().PlayerName.Contains(SNRSystemMessage))
            SaveChatLog(GetChatLog(sourcePlayer.GetClient(), chatText));
        else
            SaveSystemLog(GetSystemMessageLog(chatText));
    }
}

// 参照 => https://github.com/ykundesu/SuperNewRoles/blob/1.8.1.0/SuperNewRoles/Patches/ChatCommandPatch.cs
[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
class SendChatPatch
{
    private static bool ResetedMemo = false;
    private static readonly string LogMemoFilePath = Path.GetDirectoryName(Application.dataPath) + @"\SSP_Deputata\AmongUs_ChatMemo.log";

    static bool Prefix(ChatController __instance)
    {
        if (!SSPPlugin.ChatLog.Value) return true; // ChatLogを作成しない設定だったら判定しないようにする。

        string text = __instance.TextArea.text, addChatMemo = __instance.TextArea.text;
        bool handled = false;

        if (text.ToLower().StartsWith("/cm") || text.ToLower().StartsWith("/memo"))
        {
            handled = true;
            string soliloquy = text.ToLower().Replace("/cm ", "").Replace("/memo ", "");
            soliloquy = $"『 {soliloquy} 』";

            addChatMemo = soliloquy;
            __instance.AddChat(PlayerControl.LocalPlayer, soliloquy);
        }

        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            if (text.ToLower().StartsWith("/sgl") || text.ToLower().StartsWith("/savegamelog"))
            {
                handled = true;
                string name =
                    ReplaceUnusableStringsAsFileNames(text.Replace("/sgl ", "").Replace("/sgl", "").Replace("/savegamelog ", "").Replace("/savegamelog", ""));
                string status = GemeLogSaveAs(name);

                addChatMemo = status;
                __instance.AddChat(PlayerControl.LocalPlayer, status);
            }
        }

        SaveChatMemo(addChatMemo);

        if (handled)
        {
            __instance.TextArea.Clear();
            FastDestroyableSingleton<HudManager>.Instance.Chat.TimeSinceLastMessage = 0f;
            __instance.quickChatMenu.ResetGlyphs();
        }
        return !handled;
    }

    /// <summary>
    /// AmongUs_ChatMemo.logに自分のチャットと自視点メモを記載する。
    /// </summary>
    /// <param name="chatMemo">string : 記載する内容</param>
    /// <param name="processingRequired">true : 自分の発言, 要加工 / false : システムログ </param>
    internal static void SaveChatMemo(string chatMemo, bool processingRequired = true)
    {
        if (!SSPPlugin.ChatLog.Value) return;

        string date = DateTime.Now.ToString("[HH:mm:ss]");
        string name = PlayerControl.LocalPlayer.GetClient().PlayerName;
        string outChatMemo = processingRequired ? $"{date} {name} : {chatMemo}" : $"{date} {chatMemo}";

        if (ResetedMemo)
            File.AppendAllText(LogMemoFilePath, $"{outChatMemo}" + Environment.NewLine);
        else
        {
            ResetedMemo = true;
            File.WriteAllText(LogMemoFilePath, $"{outChatMemo}" + Environment.NewLine);
        }
    }

    /// <summary>
    /// 一個前の試合のGameLogのみを任意のファイル名で保存する
    /// </summary>
    /// <param name="name">任意のfile名</param>
    /// <returns>string : 保存処理の結果</returns>
    private static string GemeLogSaveAs(string name)
    {
        string date = DateTime.Now.ToString("yyMMdd");
        string fileName = $"{date}_The{GameCount}RoundGameLog_{name}" + ".log";
        string newFilePath = @$"{RoundGameLogFilePath}/{fileName}";

        try
        {
            using (StreamReader sr = new(ChatLogFilePath))
            {
                string allLog = sr.ReadToEnd();
                string target = $"『 {GameCount}回目の試合 開始 』";
                string log = allLog[allLog.IndexOf(target)..];

                using StreamWriter sw = new(newFilePath, false);
                sw.WriteLine(log);
            }

            return $"[ {fileName} ] に ゲームログを抜き出しました。";
        }
        catch (Exception e)
        {
            Logger.Error($"ゲームログの抜き出し保存に失敗しました : {e}");
            return "[Error] ゲームログの抜き出し保存に失敗しました。";
        }
    }

    /// <summary>
    /// stringsに含まれるファイル名に使用不可能な文字を"_"に置換する
    /// </summary>
    /// <param name="strings">ファイル名に使用したい未編集の文字列</param>
    /// <returns>ファイル名に使用できるように加工した文字列を返す</returns>
    // 参考? => https://github.com/ykundesu/SuperNewRoles/blob/1.8.1.0/SuperNewRoles/Modules/Logger.cs#L118-L131
    private static string ReplaceUnusableStringsAsFileNames(string strings)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        string fileName = strings;
        foreach (var invalid in invalidChars)
            fileName = fileName.Replace($"{invalid}", "_");
        fileName = fileName.Replace($".", "_");
        return fileName;
    }
}

internal static class SaveChatLogPatch
{
    internal static void Load()
    {
        ChatLogFileCreate();
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

    private static Dictionary<int, string> GameLogDic = new();
    private static StringBuilder NowGameLog = new();

    internal static int GameCount = 0;

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
        bool duringGamePlay = AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started;
        string name = duringGamePlay ? SaveNamesToUseChatLogInDic(sourceClient) : sourceClient.PlayerName;

        chatLog = !sourceClient.GetPlayer().IsDead()
            ? $"[{date}]        {name} :「 {chatText} 」"
            : $"[{date}] (死者) {name} :「 {chatText} 」";

        return chatLog;
    }

    /// <summary>
    /// システムメッセージをChatLogに記載する為に加工する。
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
    /// </summary>
    /// <param name="chatLog">出力するチャットログ</param>
    internal static void SaveChatLog(string chatLog)
    {
        if ((AmongUsClient.Instance?.GameState) == InnerNetClient.GameStates.Started) NowGameLog.AppendLine(chatLog);
        else File.AppendAllText(ChatLogFilePath, chatLog + Environment.NewLine);
    }

    /// <summary>
    /// システムログをファイルに出力する
    /// </summary>
    /// <param name="systemMessageLog">出力するシステムログ</param>
    /// <param name="autoFiling"> true : 辞書保存か直接的にログファイルに出力するか自動で判断する。 / false : 辞書保存にする。</param>
    internal static void SaveSystemLog(string systemMessageLog, bool autoFiling = true)
    {
        if (autoFiling)
        {
            if ((AmongUsClient.Instance?.GameState) == InnerNetClient.GameStates.Started) NowGameLog.AppendLine(systemMessageLog);
            else File.AppendAllText(ChatLogFilePath, systemMessageLog + Environment.NewLine);
        }
        else NowGameLog.AppendLine(systemMessageLog);
    }

    internal static async void AddGameLog()
    {
        string useLogString = NowGameLog.ToString();
        NowGameLog = new();

        using (StreamWriter sw = new(ChatLogFilePath, true)) await sw.WriteLineAsync(useLogString);

        if (!GameLogDic.ContainsKey(GameCount)) GameLogDic.Add(GameCount, useLogString);
    }
}

/// <summary>
/// チャットログに記載する、システムメッセージに関わるHarmonyPatchを纏めている。
/// </summary>
[HarmonyPatch]
class ChatLogHarmonyPatch
{
#pragma warning disable 8321
    // HarmonyPatchはローカル宣言で呼び出していなくても動くのに「ローカル関数 '関数名' は宣言されていますが、一度も使用されていません」と警告が出る為
    // このメソッドでは警告を表示しないようにしている
    public static void ChatLogHarmony()
    {
        if (!SSPPlugin.ChatLog.Value) return; // ChatLogを作成しない設定だったら読まないようにする。

        // ゲーム開始時に情報を記載する
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin)), HarmonyPostfix]
        static void IntroCutsceneCoBeginPostfix() => IntroCutsceneCoBeginSystemLog();

        // 会議開始
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start)), HarmonyPostfix]
        static void MeetingStartPostfix(MeetingHud __instance) => MeetingStartSystemLog(__instance);

        // 死体通報
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody)), HarmonyPostfix]
        static void ReportDeadBodyPostfix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target) => ReportDeadBodySystemLog(__instance, target);

        // 投票感知&記載(Hostのみ)
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote)), HarmonyPostfix]
        static void MeetingCastVotePostfix(byte srcPlayerId, byte suspectPlayerId) => MeetingCastVoteSystemLog(srcPlayerId, suspectPlayerId);

        // 開票(Hostのみ)
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting)), HarmonyPostfix]
        static void CheckForEndVotingPostfix(MeetingHud __instance) => MeetingCastVoteSave(__instance);

        // 会議終了(airship以外)
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp)), HarmonyPostfix]
        static void MeetingEndPostfix(ExileController __instance) => DescribeMeetingEndSystemLog(__instance.exiled);

        // 会議終了(airship)
        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn)), HarmonyPostfix]
        static void AirshipMeetingEndPostfix(ExileController __instance) => DescribeMeetingEndSystemLog(__instance.exiled);

        // キル発生時
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer)), HarmonyPostfix]
        static void MurderPlayerPostfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target) => MurderPlayerSystemLog(__instance, target);

        // 試合終了
        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp)), HarmonyPostfix]
        static void EndGamePostfix() => EndGameSystemLog();
    }
#pragma warning restore 8321
}

/// <summary>
/// チャットログに記載する、システムメッセージに関わるメソッドを纏めている。
/// </summary>
class SystemLogMethodManager
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
        if (!SSPPlugin.ChatLog.Value) return;

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
        {
            // 参考? =>https://github.com/ykundesu/SuperNewRoles/blob/1.8.1.0/SuperNewRoles/Modules/CustomOverlay.cs#L648-#L651
            string friendCode;
            if (client?.FriendCode is not null and not "") friendCode = client?.FriendCode; // フレンドコードを所持している場合
            else friendCode = "未所持"; // クライアントデータやフレンドコードがない場合, フレンドコードがブランクだった場合
            if (SSPPlugin.HideFriendCode.Value) friendCode = "**********#****"; // バニラ設定[配信者モード]が有効時フレンドコードを伏字風にする

            SaveSystemLog(GetSystemMessageLog($"{client.PlayerName}(pid:{client.GetPlayer().PlayerId})(FriendCode:{friendCode})({GetColorName(client)})({client?.PlatformData?.Platform})"));
        }

        CDToNameDic = new(); // 試合開始時に, 文字数調整した名前とClientIdと紐づけた辞書を初期化する。
        WriteForCDToNameDic(); // そして書き込む

        SaveSystemLog(GetSystemMessageLog(delimiterLine));
        SaveSystemLog("\n");
        SaveSystemLog(GetSystemMessageLog("=================Task Phase Start================="));

        SendChatPatch.SaveChatMemo(delimiterLine, false);
        SendChatPatch.SaveChatMemo("", false);
        SendChatPatch.SaveChatMemo("=================Game Info=================", false);
        SendChatPatch.SaveChatMemo($"{GameCount}回目の試合 開始", false);
        SendChatPatch.SaveChatMemo("", false);
    }

    // 会議開始
    internal static void MeetingStartSystemLog(MeetingHud __instance)
    {
        if (!SSPPlugin.ChatLog.Value) return;

        VariableManager.NumberOfMeetings++;
        SaveSystemLog(GetSystemMessageLog("=================Task Phase End================="));
        SaveSystemLog("\n");

        SaveSystemLog(GetSystemMessageLog("=================Meeting Phase Start================="));

        SaveSystemLog(GetSystemMessageLog("=================Start Meeting Info================="));

        SaveSystemLog(GetSystemMessageLog($"{GameCount}回目の試合の {VariableManager.NumberOfMeetings}回目の会議 開始"));

        SaveSystemLog(GetSystemMessageLog("=================Time of the crime and the killers and victims Info================="));

        SaveSystemLog(GetSystemMessageLog($"{VariableManager.NumberOfMeetings}ターン目, タスクフェイズ中の 犯行時刻及び 殺害者と犠牲者"));
        CrimeTimeAndKillerAndVictimLog();

        SaveSystemLog(GetSystemMessageLog("===================================================="));
        SaveSystemLog("\n");
    }

    // 死体通報
    internal static void ReportDeadBodySystemLog(PlayerControl convener, GameData.PlayerInfo target)
    {
        if (!SSPPlugin.ChatLog.Value) return;

        if (convener == null) return;
        if (target == null) SaveSystemLog(GetSystemMessageLog($"[{convener.name}] が 緊急招集しました。"));
        else SaveSystemLog(GetSystemMessageLog($"[{convener.name}] が [{target.Object.name}] の死体を通報しました。"));
    }

    /// <summary>
    /// 投票時にチャットログに投票内容を記載する。
    /// </summary>
    /// <param name="srcPlayerId">投票者のPlayerId</param>
    /// <param name="suspectPlayerId">投票先のPlayerId</param>
    internal static void MeetingCastVoteSystemLog(byte srcPlayerId, byte suspectPlayerId)
    {
        if (!SSPPlugin.ChatLog.Value) return;

        string openVoteMessage = GetOpenVoteMessage(PlayerById(srcPlayerId).GetClient().PlayerName, WhereToVoteInfo(suspectPlayerId), suspectPlayerId);
        OpenVoteSystemLog(openVoteMessage);
    }

    internal static void OpenVoteSystemLog(string openVoteMessage) => SaveSystemLog(GetSystemMessageLog(openVoteMessage));

    // 会議終了
    internal static void DescribeMeetingEndSystemLog(GameData.PlayerInfo exiled)
    {
        if (!SSPPlugin.ChatLog.Value) return;

        SaveSystemLog("\n");
        SaveSystemLog(GetSystemMessageLog("=================End Meeting Info================="));
        if (exiled != null && exiled.Object == null) exiled = null;
        SaveSystemLog(GetSystemMessageLog($"{GameCount}回目の試合の {VariableManager.NumberOfMeetings}回目の会議 終了"));
        if (exiled == null) SaveSystemLog(GetSystemMessageLog($"誰も追放されませんでした。"));
        else
        {
            SaveSystemLog(GetSystemMessageLog($"[ {exiled.Object.name} ] が追放されました。"));
            if (!VariableManager.AllladyVictimDic.ContainsKey(exiled.Object.PlayerId))
                VariableManager.AllladyVictimDic.Add(exiled.Object.PlayerId, exiled.Object); // 追放された人を死亡者辞書に追加する
        }

        // 投票情報記載
        OpenVoteDecoding();

        SaveSystemLog(GetSystemMessageLog("=================Time of the crime and the killers and victims Info================="));

        SaveSystemLog(GetSystemMessageLog($"{VariableManager.NumberOfMeetings}ターン目, ミーティングフェイズ中の 犯行時刻及び 殺害者と犠牲者"));
        CrimeTimeAndKillerAndVictimLog();
        SaveSystemLog(GetSystemMessageLog("===================================================="));
        SaveSystemLog(GetSystemMessageLog("=================Meeting Phase End================="));
        SaveSystemLog("\n");

        SaveSystemLog(GetSystemMessageLog("=================Task Phase Start================="));
    }


    internal static void CrimeTimeAndKillerAndVictimLog()
    {
        if (!SSPPlugin.ChatLog.Value) return;

        foreach (KeyValuePair<DateTime, (ClientData, ClientData)> kvp in VariableManager.CrimeTimeAndKillersAndVictims)
        {
            ClientData killerClient = kvp.Value.Item1;
            ClientData victimClient = kvp.Value.Item2;
            PlayerControl victimPlayer = victimClient.GetPlayer();
            byte plId = victimPlayer.PlayerId;

            if (kvp.Key == null && killerClient == null && victimClient == null) continue;

            string crimeTime = kvp.Key != null ? kvp.Key.ToString("HH:mm:ss") : "死亡時刻不明";
            string killerName = killerClient.PlayerName ?? "不明";
            string victimName = victimClient.PlayerName ?? "身元不明";
            string victimColor = victimClient != null ? GetColorName(victimClient) : "";

            SaveSystemLog(GetSystemMessageLog($"犯行時刻:[{crimeTime}] 殺害者:[{killerName}] 犠牲者:[{victimName} ({victimColor})]"));

            if (!VariableManager.AllladyVictimDic.ContainsKey(plId)) VariableManager.AllladyVictimDic.Add(plId, victimPlayer);
        }
        VariableManager.CrimeTimeAndKillersAndVictims = new();

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.IsAlive()) { VariableManager.AllladyVictimDic.Remove(player.PlayerId); continue; } // 生きているなら死亡者辞書から削除し, 次のプレイヤー処理へ
            if (VariableManager.AllladyVictimDic.ContainsKey(player.PlayerId)) { Logger.Info("うごいてる?"); continue; } // (死んでいて, )死亡者辞書に含まれているなら, 次のプレイヤー処理へ

            // 以下 死んでいて, 死亡者辞書に含まれていない時 の処理
            try { VariableManager.AllladyVictimDic.Add(player.PlayerId, player); } // 死亡者辞書に追加する
            catch (Exception e) { Logger.Error($"死亡者辞書に保存する際, エラーが発生しました。 : {e}"); }

            ClientData victimClient = player.GetClient();
            string victimName = victimClient.PlayerName ?? "身元不明";
            string victimColor = victimClient != null ? GetColorName(victimClient) : "";
            SaveSystemLog(GetSystemMessageLog($"犯行時刻:[死亡時刻不明] 殺害者:[不明] 犠牲者:[{victimName} ({victimColor})]"));
        }
    }

    // キル発生時
    internal static void MurderPlayerSystemLog(PlayerControl Killer, PlayerControl victim)
    {
        if (!SSPPlugin.ChatLog.Value) return;

        SaveSystemLog(GetSystemMessageLog($"[ {Killer.name} ] が [ {victim.name} ]を殺害しました。"));
        VariableManager.CrimeTimeAndKillersAndVictims[DateTime.Now] = (Killer.GetClient(), victim.GetClient());
    }

    // 試合終了
    internal static void EndGameSystemLog()
    {
        if (!SSPPlugin.ChatLog.Value) return;

        SaveSystemLog(GetSystemMessageLog(delimiterLine), false);
        SaveSystemLog(GetSystemMessageLog("=================End Game Info================="), false);
        SaveSystemLog(GetSystemMessageLog($"{GameCount}回目の試合 終了"), false);
        SaveSystemLog(GetSystemMessageLog(delimiterLine), false);

        AddGameLog();

        SendChatPatch.SaveChatMemo("=================End Game Info=================", false);
        SendChatPatch.SaveChatMemo($"{GameCount}回目の試合 終了", false);
        SendChatPatch.SaveChatMemo(delimiterLine, false);
    }

    /// <summary>
    /// 投票logを作成するメソッドを纏めている
    /// </summary>
    internal static class VoteLogMethodManager
    {
        /// <summary>
        /// 投票状況を辞書に格納する。
        /// ClientIdを保存していない理由は, スキップや無投票等がPlayerIdを流用している為、正確な情報を保存できなくなるから。
        /// 参考=>https://github.com/yukieiji/ExtremeRoles/blob/55b1bb54557cf036de2ec7d64d709dde673e17ec/ExtremeRoles/Patches/Meeting/MeetingHudPatch.cs#L277-L293
        /// </summary>
        /// <param name="__instance"></param>
        internal static void MeetingCastVoteSave(MeetingHud __instance)
        {
            if (!SSPPlugin.ChatLog.Value) return;

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

            foreach (KeyValuePair<byte, byte> voteResult in VariableManager.ResultsOfTheVoteCount)
            {
                byte srcPlayerId = voteResult.Key;
                byte votedForPlayerId = voteResult.Value;
                PlayerControl srcPlayerControl = PlayerById(srcPlayerId);
                if (srcPlayerControl.IsDead()) continue;
                string srcPlayerName = srcPlayerControl.GetClient().PlayerName;
                string votedForPlayerName = WhereToVoteInfo(votedForPlayerId);
                string openVoteMessage = GetOpenVoteMessage(srcPlayerName, votedForPlayerName, votedForPlayerId);
                OpenVoteSystemLog(openVoteMessage);
            }

            SaveSystemLog(GetSystemMessageLog("=================Open Votes Info End================="));
            SaveSystemLog("\n");

            VariableManager.ResultsOfTheVoteCount = new Dictionary<byte, byte>();
        }

        /// <summary>
        /// 投票先の情報をplayerIdから解読する。
        /// </summary>
        /// <param name="playerId">投票先</param>
        /// <returns>string : 投票先(名前や投票状況)</returns>
        internal static string WhereToVoteInfo(byte playerId)
        {
            return playerId switch
            {
                252 => "???",
                253 => "スキップ",
                254 => "無投票",
                255 => "未投票",
                _ => PlayerById(playerId).GetClient().PlayerName,
            };
        }

        /// <summary>
        /// バラバラな[投票者の名前]と[投票先の情報]を、投票先PlIdを使用しformatを判定して、文章として構成する。
        /// </summary>
        /// <param name="srcPlayerName">投票者の名前</param>
        /// <param name="whereToVoteInfo">投票先の情報(スキップ等 反映済み)</param>
        /// <param name="votedForPlayerId">投票先のPlId(format変更判別に使用 投票先の情報はここから取得しない)</param>
        /// <returns>文章化された投票情報</returns>
        internal static string GetOpenVoteMessage(string srcPlayerName, string whereToVoteInfo, byte votedForPlayerId)
        {
            string messageFormat = "";
            messageFormat = votedForPlayerId switch
            {
                252 or 253 => "[{0}] が [{1}] に投票しました。",
                254 => "[{0}] は [{1}] でした。",
                255 => "[{0}] は [{1}] です。",
                _ => "[{0}] が [{1}] に投票しました。",
            };
            return Format(messageFormat, srcPlayerName, whereToVoteInfo);
        }
    }
}
