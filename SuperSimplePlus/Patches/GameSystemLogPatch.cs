using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InnerNet;
using static System.String;
using static SuperSimplePlus.Helpers;
using static SuperSimplePlus.Patches.SaveChatLogPatch;

namespace SuperSimplePlus.Patches;
/// <summary>
/// チャットログに記載する、システムメッセージに関わるメソッドを纏めている。
/// </summary>
class GameSystemLogPatch
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
        GameLogManager.GameCount++;
        SaveSystemLog(GetSystemMessageLog(delimiterLine));

        SaveSystemLog(GetSystemMessageLog("=================Game Info================="));
        SaveSystemLog(GetSystemMessageLog($"{GameLogManager.GameCount}回目の試合 開始"));
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

        SaveChatMemo(delimiterLine, false);
        SaveChatMemo("", false);
        SaveChatMemo("=================Game Info=================", false);
        SaveChatMemo($"{GameLogManager.GameCount}回目の試合 開始", false);
        SaveChatMemo("", false);
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

        SaveSystemLog(GetSystemMessageLog($"{GameLogManager.GameCount}回目の試合の {VariableManager.NumberOfMeetings}回目の会議 開始"));

        SaveSystemLog(GetSystemMessageLog("=================Time of the crime and the killers and victims Info================="));

        SaveSystemLog(GetSystemMessageLog($"{VariableManager.NumberOfMeetings}ターン目, タスクフェイズ中の 犯行時刻及び 殺害者と犠牲者"));
        CrimeTimeAndKillerAndVictimLog();

        SaveSystemLog(GetSystemMessageLog("===================================================="));
        SaveSystemLog("\n");
    }

    // 死体通報
    internal static void ReportDeadBodySystemLog(PlayerControl convener, NetworkedPlayerInfo target)
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

        string openVoteMessage = VoteLogMethodManager.GetOpenVoteMessage(PlayerById(srcPlayerId).GetClient().PlayerName, VoteLogMethodManager.WhereToVoteInfo(suspectPlayerId), suspectPlayerId);
        OpenVoteSystemLog(openVoteMessage);
    }

    internal static void OpenVoteSystemLog(string openVoteMessage) => SaveSystemLog(GetSystemMessageLog(openVoteMessage));

    // 会議終了
    internal static void DescribeMeetingEndSystemLog(NetworkedPlayerInfo exiled)
    {
        if (!SSPPlugin.ChatLog.Value) return;

        SaveSystemLog("\n");
        SaveSystemLog(GetSystemMessageLog("=================End Meeting Info================="));
        if (exiled != null && exiled.Object == null) exiled = null;
        SaveSystemLog(GetSystemMessageLog($"{GameLogManager.GameCount}回目の試合の {VariableManager.NumberOfMeetings}回目の会議 終了"));
        if (exiled == null) SaveSystemLog(GetSystemMessageLog($"誰も追放されませんでした。"));
        else
        {
            SaveSystemLog(GetSystemMessageLog($"[ {exiled.Object.name} ] が追放されました。"));
            if (!VariableManager.AllladyVictimDic.ContainsKey(exiled.Object.PlayerId))
                VariableManager.AllladyVictimDic.Add(exiled.Object.PlayerId, exiled.Object); // 追放された人を死亡者辞書に追加する
        }

        // 投票情報記載
        VoteLogMethodManager.OpenVoteDecoding();

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
            if (VariableManager.AllladyVictimDic.ContainsKey(player.PlayerId)) continue; // (死んでいて, )死亡者辞書に含まれているなら, 次のプレイヤー処理へ

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
        SaveSystemLog(GetSystemMessageLog($"{GameLogManager.GameCount}回目の試合 終了"), false);
        SaveSystemLog(GetSystemMessageLog(delimiterLine), false);

        GameLogManager.AddGameLog();

        SaveChatMemo("=================End Game Info=================", false);
        SaveChatMemo($"{GameLogManager.GameCount}回目の試合 終了", false);
        SaveChatMemo(delimiterLine, false);
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
