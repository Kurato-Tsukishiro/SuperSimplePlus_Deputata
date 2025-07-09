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

class RecordingChatPatch
{
    /// <summary> SNRのシステムメッセージに含まれる文字列 </summary>
    internal const string SNRSystemMessage
        = "<color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color>";

    // 参考=>https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Patches/ChatHandlerPatch.cs
    /// <summary>
    /// チャットに流れた文字をチャットログを作成するメソッドに渡す。
    /// SNRのコマンドの返答等SNR側のSystemMessageの場合はSystemMessageとして反映する。
    /// </summary>
    /// <param name="sourcePlayer">チャット送信者</param>
    /// <param name="chatText">チャット内容</param>
    internal static void MonitorChat(PlayerControl sourcePlayer, string chatText)
    {
        if (!sourcePlayer.GetClient().PlayerName.Contains(SNRSystemMessage))
            SaveChatLog(GetChatLog(sourcePlayer.GetClient(), chatText));
        else
            SaveSystemLog(GetSystemMessageLog(chatText));
    }

    // 参照 => https://github.com/ykundesu/SuperNewRoles/blob/1.8.1.0/SuperNewRoles/Patches/ChatCommandPatch.cs
    /// <summary> チャットコマンドの制御を行う </summary>
    /// <param name="__instance"></param>
    /// <param name="handled">チャットの送信を無効にするか</param>
    internal static void SendChatPrefix(ChatController __instance, out bool handled)
    {
        string text = __instance.freeChatField.textArea.text, addChatMemo = __instance.freeChatField.textArea.text;
        handled = false;

        // 通常command
        if (text.ToLower().StartsWith("/banlistlnquiry") || text.ToLower().StartsWith("/bll"))
        {
            handled = true;
            Dictionary<int, string> warningTextDic = new();

            foreach (ClientData cd in AmongUsClient.Instance.allClients)
            {
                (var isTaregt, var friendCode) = ImmigrationCheck.DenyEntryToFriendCode(cd);

                if (isTaregt)
                {
                    var warningText = $"{cd.PlayerName}は, {(friendCode != "未所持" ? $"BAN対象のコード{friendCode}を所持しています" : "フレンドコードを所持していません")}。";

                    if (warningTextDic.ContainsKey(cd.Id)) warningTextDic.Add(cd.Id, warningText);
                    else warningTextDic[cd.Id] = warningText;
                }
            }

            string warningMessage = "";
            foreach (KeyValuePair<int, string> kvp in warningTextDic) { warningMessage += $"{kvp.Value}\n"; }
            if (warningMessage == "")
            {
                __instance.AddChat(PlayerControl.LocalPlayer, $"<align={"left"}><color=#89c3eb><size=150%>Infomation</size></color><size=80%>\n現在, BANList対象者は入室しておりません。</size></align>");
            }
            else
            {
                __instance.AddChat(PlayerControl.LocalPlayer, $"<align={"left"}><color=#F2E700><size=150%>警告!</size></color><size=80%>\n{warningMessage}</size></align>");
            }
        }

        if (!GameLogManager.IsValidChatLog) return; // ChatLogを作成しない設定だったら判定しないようにする。
        if (text.ToLower().StartsWith("/cm") || text.ToLower().StartsWith("/memo"))
        {
            handled = true;
            string soliloquy = text.ToLower().Replace("/cm ", "").Replace("/memo ", "");
            soliloquy = $"『 {soliloquy} 』";

            addChatMemo = soliloquy;
            __instance.AddChat(PlayerControl.LocalPlayer, soliloquy);
        }
        else if (text.ToLower().StartsWith("/ngc") || text.ToLower().StartsWith("/nowgamecount"))
        {
            handled = true;
            string gameCountAnnounce = addChatMemo = Format(ModTranslation.GetString("NowGameCountAnnounce"), GameLogManager.GameCount);
            __instance.AddChat(PlayerControl.LocalPlayer, gameCountAnnounce);
        }

        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            if (text.ToLower().StartsWith("/sgl") || text.ToLower().StartsWith("/savegamelog"))
            {
                // 参照 => https://github.com/ykundesu/SuperNewRoles/blob/1.8.1.2/SuperNewRoles/Modules/ModTranslation.cs

                handled = true;

                string removeCommandText = text.Replace("/sgl ", "").Replace("/sgl", "").Replace("/savegamelog ", "").Replace("/savegamelog", "");
                string[] commandArray = removeCommandText.Replace(", ", ",").Split(','); // チャットコマンドの引数の区切りを','としている為、これで引数を抜き出せる


                string saveStatusMessage = IsValidatingCommand(commandArray, out int index, out int logNum)
                    ? GameLogManager.ManuallySaveGameLog(index >= 0 ? commandArray[index] : Empty, logNum) // ファイル名が入力されていない(index == -1)場合は空の文字列を渡す
                    : $"[Error(SaveGameLog)] : 有効なコマンド列でなかった為,抜き出しに失敗しました。\n( {text} )";

                addChatMemo = saveStatusMessage;
                __instance.AddChat(PlayerControl.LocalPlayer, saveStatusMessage);

                // 有効なコマンドかの判定と、コマンドの引数の取得を行う
                bool IsValidatingCommand(string[] array, out int index, out int count)
                {
                    index = -1; // -1 の時ファイル名指定なしとする
                    count = 0;

                    if (array.Length <= 0) return false;

                    if (int.TryParse(array[0], out int parse))
                    {
                        count = parse; // 引数1を取得対象のログの指定として取得
                        index = 1; // 引数2をファイル名として取得
                    }
                    else
                    {
                        count = GameLogManager.GameCount; // 指定されていない場合は 最終試合を取得対象とする。
                        index = 0; // 引数1をファイル名として取得
                    }

                    if (index >= commandArray.Length) index = -1; // 範囲外エラーが発生する場合は、ファイル名の引数が渡されなかったものとみなす
                    return true; // 配列 範囲チェック (ファイル名が正常に指定されているか
                }
            }
        }

        SaveChatMemo(addChatMemo);
    }
}

internal static class SaveChatLogPatch
{
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
        if ((AmongUsClient.Instance?.GameState) == InnerNetClient.GameStates.Started) GameLogManager.NowGameLog.AppendLine(chatLog);
        else File.AppendAllText(GameLogManager.ChatLogFilePath, chatLog + Environment.NewLine);
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
            if ((AmongUsClient.Instance?.GameState) == InnerNetClient.GameStates.Started) GameLogManager.NowGameLog.AppendLine(systemMessageLog);
            else File.AppendAllText(GameLogManager.ChatLogFilePath, systemMessageLog + Environment.NewLine);
        }
        else GameLogManager.NowGameLog.AppendLine(systemMessageLog);
    }

    private static bool ResetedMemo = false;
    private static readonly string LogMemoFilePath = Path.GetDirectoryName(Application.dataPath) + @"\SSP_Deputata\AmongUs_ChatMemo.log";

    /// <summary>
    /// AmongUs_ChatMemo.logに自分のチャットと自視点メモを記載する。
    /// </summary>
    /// <param name="chatMemo">string : 記載する内容</param>
    /// <param name="processingRequired">true : 自分の発言, 要加工 / false : システムログ </param>
    internal static void SaveChatMemo(string chatMemo, bool processingRequired = true)
    {
        if (!GameLogManager.IsValidChatLog) return;

        string date = DateTime.Now.ToString("[HH:mm:ss]");
        string outChatMemo = processingRequired ? $"{date} {PlayerControl.LocalPlayer?.GetClient().PlayerName} : {chatMemo}" : $"{date} {chatMemo}";

        if (ResetedMemo)
            File.AppendAllText(LogMemoFilePath, $"{outChatMemo}" + Environment.NewLine);
        else
        {
            ResetedMemo = true;
            File.WriteAllText(LogMemoFilePath, $"{outChatMemo}" + Environment.NewLine);
        }
    }
}
