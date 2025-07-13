using System.Collections.Generic;
using System.Linq;
using InnerNet;

namespace SuperSimplePlus.Patches;

internal static class FriendCodeImmigrationPatch
{
    internal static void ChatCommand(ChatController __instance, out bool handled)
    {
        string text = __instance.freeChatField.textArea.text;
        handled = false;

        if (RecordingChatPatch.StartsWithCommands(text, RecordingChatPatch.CommandAliases["BanListinquiry"]))
        {
            handled = true;
            Dictionary<int, string> warningTextDic = new();

            foreach (ClientData cd in AmongUsClient.Instance.allClients)
            {
                if (Modules.ImmigrationCheck.DenyEntryToFriendCode(cd))
                {
                    var friendCode = Modules.ImmigrationCheck.FriendCodeFormatString(cd);
                    var warningText = $"{cd.PlayerName}は, {(Modules.ImmigrationCheck.HasFriendCode(cd) ? $"BAN対象のコード{friendCode}を所持しています" : "フレンドコードを所持していません")}。";

                    warningTextDic[cd.Id] = warningText;
                }
            }

            __instance.AddChat(PlayerControl.LocalPlayer, BuildPlayerWarningMessage(in warningTextDic));
        }
    }

    internal static void OnPlayerJoined_postfix(ClientData client)
    {
        if (client == null)
        {
            Logger.Error($"対象が存在しません");
            return;
        }

        var isTaregt = Modules.ImmigrationCheck.DenyEntryToFriendCode(client);
        var friendCode = Modules.ImmigrationCheck.FriendCodeFormatString(client);

        Logger.Info($"[{client.PlayerName}], ClientId : {client.Id}, Platform:{client.PlatformData.Platform}, FriendCode : {friendCode}({(isTaregt ? '×' : '〇')})", "OnPlayerJoined");

        if (!isTaregt) return;

        var canChat = FastDestroyableSingleton<HudManager>.Instance != null && FastDestroyableSingleton<HudManager>.Instance.Chat != null;

        if (AmongUsClient.Instance.AmHost && SSPPlugin.FriendCodeBan.Value)
        {
            AmongUsClient.Instance.KickPlayer(client.Id, ban: true); // 入室者がフレンドコードを未所持の場合 又は 入室者のコードが辞書に登録されている場合 BAN をする

            var message = $"BANList対象者 : {client.PlayerName} ( {friendCode} ) のBANを実行しました。";
            if (canChat) FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, message);
            Logger.Info(message);
        }
        else //ゲスト 又は, ホストで機能が無効な場合
        {
            if (canChat)
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"<align={"left"}><color=#F2E700><size=150%>警告!</size></color><size=80%>\n{client.PlayerName}は, {(Modules.ImmigrationCheck.HasFriendCode(client) ? $"BAN対象のコード{friendCode}を所持しています" : "フレンドコードを所持していません")}。</size></align>");
        }
    }

    /// <summary>入室時に既に参加していたプレイヤーをログに記載する</summary>
    internal static void RecordsOfExistingPlayer()
    {
        if (AmongUsClient.Instance.AmHost) return;

        Dictionary<int, string> participantDic = new();
        // Dictionary<int, string> warningTextDic = new();

        foreach (ClientData cd in AmongUsClient.Instance.allClients)
        {
            var isTaregt = Modules.ImmigrationCheck.DenyEntryToFriendCode(cd);
            var friendCode = Modules.ImmigrationCheck.FriendCodeFormatString(cd);

            var dicPage = $"[{cd.PlayerName}], ClientId : {cd.Id}, Platform:{cd.PlatformData.Platform}, FriendCode : {friendCode}({(isTaregt ? '×' : '〇')})";
            // var warningText = "";

            participantDic[cd.Id] = dicPage;

            /*if (isTaregt)
            {
                warningText = $"{cd.PlayerName}は, {(Modules.ImmigrationCheck.HasFriendCode(cd) ? $"BAN対象のコード{friendCode}を所持しています" : "フレンドコードを所持していません")}。";

                warningTextDic[cd.Id] = warningText;
            }*/
        }


        // FIXME : 入室タイミングでのチャット表示は ``FastDestroyableSingleton<HudManager>.Instance.Chat`` が null の為使用できない。タイマーとかで実行タイミングずらして表示できるようにする
        /*
        if (FastDestroyableSingleton<HudManager>.Instance != null && FastDestroyableSingleton<HudManager>.Instance.Chat != null)
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, CreatePlayerWarningMessage(in warningTextDic));
        */


        Logger.Info($"|:========== 既入室者の記録 Start ==========:|", "AmongUsClientOnPlayerJoindPatch");
        foreach (KeyValuePair<int, string> kvp in participantDic) Logger.Info(kvp.Value, "OnPlayerJoined");

        Logger.Info($"|:========== 既入室者の記録 End ==========:|", "AmongUsClientOnPlayerJoindPatch");
    }

    // 参考 => https://g.co/gemini/share/a44da4e0b41a
    /// <summary>既入室者の情報/警告メッセージを組み立てる</summary>
    /// <param name="warningTextDic">Key =>警戒対象者のClient Id / Value => 警告テキストの辞書</param>
    /// <returns>チャットに表示する情報/警告メッセージ</returns>
    private static string BuildPlayerWarningMessage(in Dictionary<int, string> warningTextDic)
    {
        // 警告メッセージ用の変数を定義
        string title;
        string color;
        string content;

        // warningTextDicに警告があるかどうかで処理を分岐
        if (warningTextDic.Any())
        {
            // 警告がある場合のタイトル、色、内容を設定
            title = "警告!";
            color = "#F2E700";
            // string.Joinで効率的に文字列を結合し、最後に改行を追加
            content = string.Join("\n", warningTextDic.Values) + "\n";
        }
        else
        {
            // 警告がない場合のタイトル、色、内容を設定
            title = "Infomation";
            color = "#89c3eb";
            content = "現在, BANList対象者は入室しておりません。";
        }

        // 最後に組み立てた情報を使ってAddChatを一度だけ呼び出す
        string chatMessage = $"<align=left><color={color}><size=150%>{title}</size></color><size=80%>\n{content}</size></align>";

        return chatMessage;
    }
}
