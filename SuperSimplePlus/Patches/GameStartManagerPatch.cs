using System;
using System.Collections.Generic;
using HarmonyLib;
using InnerNet;
namespace SuperSimplePlus.Patches;
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public class GameStartManagerUpdatePatch
{
    public static void Postfix(GameStartManager __instance)
    {
        if (AmongUsClient.Instance.AmHost) return;
        if (!(SSPPlugin.NotPCKick.Value || SSPPlugin.NotPCBan.Value)) return;

        foreach (ClientData p in AmongUsClient.Instance.allClients)
        {
            if (p.PlatformData.Platform is not Platforms.StandaloneEpicPC and not Platforms.StandaloneSteamPC)
                AmongUsClient.Instance.KickPlayer(p.Id, ban: SSPPlugin.NotPCBan.Value); // 第2引数が trueの時 BAN / falseの時Kick
        }
    }

    //参考=>https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Patches/ShareGameVersionPatch.cs
    public static void Prefix(GameStartManager __instance) => __instance.MinPlayers = 1;
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
public class AmongUsClientOnPlayerJoindPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        string friendCode = client?.FriendCode;
        Logger.Info($"{client.PlayerName} が入室しました。[PlayerInfo] \"ID:{client.Id} Platform:{client.PlatformData.Platform} FriendCode:{(!SSPPlugin.DisplayFriendCode.Value ? friendCode : "**********#****")}\"", "OnPlayerJoined");
    }
}

//参考=>https://github.com/haoming37/TheOtherRoles-GM-Haoming/blob/haoming-main/TheOtherRoles/Patches/GameStartManagerPatch.cs
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
public class AmongUsClientOnPlayerLeftPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client, [HarmonyArgument(1)] DisconnectReasons reason) =>
        Logger.Info($"PlayerName: \"{client.PlayerName}(ID:{client.Id})({client.PlatformData.Platform})\" Left (Reason: {reason})", "OnPlayerLeft");
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
    }
}
