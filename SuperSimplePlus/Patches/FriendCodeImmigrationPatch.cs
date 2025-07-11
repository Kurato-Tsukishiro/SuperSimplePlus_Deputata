using System.Collections.Generic;
using InnerNet;

namespace SuperSimplePlus.Patches;

internal static class FriendCodeImmigrationPatch
{
    internal static void ChatCommand(ChatController __instance, out bool handled)
    {
        string text = __instance.freeChatField.textArea.text;
        handled = false;

        if (RecordingChatPatch.StartsWithCommands(text, RecordingChatPatch.CommandAliases["BanListLnquiry"]))
        {
            handled = true;
            Dictionary<int, string> warningTextDic = new();

            foreach (ClientData cd in AmongUsClient.Instance.allClients)
            {
                if (Modules.ImmigrationCheck.DenyEntryToFriendCode(cd))
                {
                    var friendCode = Modules.ImmigrationCheck.FriendCodeFormatString(cd);
                    var warningText = $"{cd.PlayerName}は, {(Modules.ImmigrationCheck.HasFriendCode(cd) ? $"BAN対象のコード{friendCode}を所持しています" : "フレンドコードを所持していません")}。";

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
}
