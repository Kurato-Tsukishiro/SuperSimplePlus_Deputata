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
                (var isTaregt, var friendCode) = Modules.ImmigrationCheck.DenyEntryToFriendCode(cd);

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
    }

    internal static void OnPlayerJoined_postfix(ClientData cd)
    {
        (var isTaregt, var friendCode) = Modules.ImmigrationCheck.DenyEntryToFriendCode(cd, true);
        var isCodeOK = isTaregt ? '×' : '〇';

        Logger.Info($"[{cd.PlayerName}], ClientId : {cd.Id}, Platform:{cd.PlatformData.Platform}, FriendCode : {friendCode}({isCodeOK})", "OnPlayerJoined");

        if (!isTaregt) return;

        if (!(AmongUsClient.Instance.AmHost && SSPPlugin.FriendCodeBan.Value)) //ゲスト 又は, ホストで機能が無効な場合
            FastDestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(PlayerControl.LocalPlayer, $"<align={"left"}><color=#F2E700><size=150%>警告!</size></color><size=80%>\n{cd.PlayerName}は, {(friendCode != "未所持" ? $"BAN対象のコード{friendCode}を所持しています" : "フレンドコードを所持していません")}。</size></align>");
    }
}
