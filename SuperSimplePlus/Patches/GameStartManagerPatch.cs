using System;
using System.IO;
using System.Collections.Generic;
using HarmonyLib;
using InnerNet;
namespace SuperSimplePlus.Patches;
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public class GameStartManagerUpdatePatch
{
    public static void Postfix(GameStartManager __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!(SSPPlugin.NotPCKick.Value || SSPPlugin.NotPCBan.Value)) return;

        foreach (ClientData c in AmongUsClient.Instance.allClients)
        {
            if (c.PlatformData.Platform is not Platforms.StandaloneEpicPC and not Platforms.StandaloneSteamPC)
                AmongUsClient.Instance.KickPlayer(c.Id, ban: SSPPlugin.NotPCBan.Value); // 第2引数が trueの時 BAN / falseの時Kick
        }
    }

    //参考=>https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Patches/ShareGameVersionPatch.cs
    public static void Prefix(GameStartManager __instance) => __instance.MinPlayers = 1;
}

[HarmonyPatch]
internal class JoindPatch
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined)), HarmonyPostfix]
    internal static void OnGameJoined_Postfix(AmongUsClient __instance)
    {
        if (AmongUsClient.Instance.AmHost) return;

        Dictionary<int, string> participantDic = new();

        foreach (ClientData cd in AmongUsClient.Instance.allClients)
        {
            (var isTaregt, var friendCode) = ImmigrationCheck.DenyEntryToFriendCode(cd);
            var isCodeOK = isTaregt ? '×' : '〇';
            var dicPage = $"[{cd.PlayerName}], ClientId : {cd.Id}, Platform:{cd.PlatformData.Platform}, FriendCode : {friendCode}({isCodeOK})";

            if (participantDic.ContainsKey(cd.Id)) participantDic.Add(cd.Id, dicPage);
            else participantDic[cd.Id] = dicPage;

            if (isTaregt)
            {
                string warning = $"<align={"left"}><color=#F2E700><size=150%>警告!</size></color>\n{cd.PlayerName}は, {(friendCode != "未所持" ? $"BAN対象のコード{friendCode}を所持しています" : "フレンドコードを所持していません")}。</align>";
                FastDestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(PlayerControl.LocalPlayer, warning);
            }
        }

        Logger.Info($"|:========== 既入室者の記録 Start ==========:|", "AmongUsClientOnPlayerJoindPatch");
        foreach (KeyValuePair<int, string> kvp in participantDic) Logger.Info(kvp.Value, "OnPlayerJoined");
        Logger.Info($"|:========== 既入室者の記録 End ==========:|", "AmongUsClientOnPlayerJoindPatch");
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined)), HarmonyPostfix]
    internal static void OnPlayerJoined_Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        (var isTaregt, var friendCode) = ImmigrationCheck.DenyEntryToFriendCode(client, true);
        var isCodeOK = isTaregt ? '×' : '〇';

        Logger.Info($"[{client.PlayerName}], ClientId : {client.Id}, Platform:{client.PlatformData.Platform}, FriendCode : {friendCode}({isCodeOK})", "OnPlayerJoined");

        if (!isTaregt) return;

        if (!(AmongUsClient.Instance.AmHost && SSPPlugin.FriendCodeBan.Value)) //ゲスト 又は, ホストで機能が無効な場合
            FastDestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(PlayerControl.LocalPlayer, $"<align={"left"}><color=#F2E700><size=150%>警告!</size></color>\n{client.PlayerName}は, {(friendCode != "未所持" ? $"BAN対象のコード{friendCode}を所持しています" : "フレンドコードを所持していません")}。</align>");
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
public class AmongUsClientOnPlayerLeftPatch
{
    //参考=>https://github.com/haoming37/TheOtherRoles-GM-Haoming/blob/haoming-main/TheOtherRoles/Patches/GameStartManagerPatch.cs
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client, [HarmonyArgument(1)] DisconnectReasons reason)
    {
        Logger.Info($"PlayerName: \"{client.PlayerName}(ID:{client.Id})({client.PlatformData.Platform})\" Left (Reason: {reason})", "OnPlayerLeft");

        if (!AmongUsClient.Instance.AmHost) return;
        if (reason == DisconnectReasons.Banned) WriteBunReport(client);
    }

    private static void WriteBunReport(ClientData client)
    {
        (var isAllladyTaregt, var friendCode) = ImmigrationCheck.DenyEntryToFriendCode(client);

        if (isAllladyTaregt) return; // 既にBunListに登録されている場合は記載しない。
        // PC以外BANが有効で, Steam・Epic でない場合, 自動BANなので記載しない。
        if (SSPPlugin.NotPCBan.Value && (client.PlatformData.Platform is not Platforms.StandaloneEpicPC and not Platforms.StandaloneSteamPC)) return;
        string bunReportPath = @$"{SaveChatLogPatch.SSPDFolderPath}" + @$"BenReport.log";

        Logger.Info($"BANListに登録していない人の手動BANを行った為, 保存します。 => {client.PlayerName} : {friendCode}");
        string log = $"登録日時 : {DateTime.Now:yyMMdd_HHmm}, 登録者 : {client.PlayerName} ( {client?.FriendCode} ), プラットフォーム : {client.PlatformData.Platform}";
        File.AppendAllText(bunReportPath, log + Environment.NewLine);
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
public class GameStartManagerStartPatch
{
    public static void Postfix() => VariableManager.ClearAndReload();
}

/// <summary>
/// ClearAndReloadを行う変数の管理場所。
/// 本来此処に置くべき物ではないが、初期化が必要な変数がそこまで増えると思えない為、
/// 逆に同じ場所で宣言・初期化した方が分かりやすいと思い此処に置いた。
/// </summary>
public class VariableManager
{
    /// <summary>
    /// 会議階位数を保存
    /// </summary>
    public static int NumberOfMeetings;

    /// <summary>
    /// 死亡時刻と死亡者の名前を保存
    /// 参照 => https://github.com/ykundesu/SuperNewRoles/blob/33647263215a4097066c9f6345e5303fc73b42f3/SuperNewRoles/Roles/CrewMate/DyingMessenger.cs
    /// </summary>
    internal static Dictionary<DateTime, (ClientData, ClientData)> CrimeTimeAndKillersAndVictims;
    internal static Dictionary<byte, PlayerControl> AllladyVictimDic;

    /// <summary>
    /// 投票者と投票先を保存
    /// </summary>
    internal static Dictionary<byte, byte> ResultsOfTheVoteCount;
    public static void ClearAndReload()
    {
        NumberOfMeetings = 0;
        CrimeTimeAndKillersAndVictims = new();
        AllladyVictimDic = new();
        ResultsOfTheVoteCount = new();
        Helpers.IdControlDic = new();
        Helpers.CDToNameDic = new();
        SaveChatLogPatch.AddGameLog();
    }
}
