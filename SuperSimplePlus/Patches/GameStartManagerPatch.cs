using System;
using HarmonyLib;
using InnerNet;
using SuperSimplePlus.Modules;

namespace SuperSimplePlus.Patches;
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public class GameStartManagerUpdatePatch
{
    // FIXME : ネスト深すぎ() LINQ形式に変えられない? あるいは early return & break形式()
    public static void Postfix(GameStartManager __instance)
    {
        if (AmongUsClient.Instance.AmHost && (SSPPlugin.NotPCKick.Value || SSPPlugin.NotPCBan.Value))
        {
            foreach (InnerNet.ClientData p in AmongUsClient.Instance.allClients)
            {
                if (p.PlatformData.Platform is not Platforms.StandaloneEpicPC and not Platforms.StandaloneSteamPC)
                {
                    //[NotPCBan.Value] が <true> の時はバン、 [NotPCBan.Value] が <false> の時はキックをするコードに変わる。
                    //kickにするコードに変わっても [NotPCKick.Value] が <true> でない時はここのコードに入らない
                    AmongUsClient.Instance.KickPlayer(p.Id, SSPPlugin.NotPCBan.Value);
                }
            }
        }
    }

    //参考=>https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Patches/ShareGameVersionPatch.cs
    public static void Prefix(GameStartManager __instance) => __instance.MinPlayers = 1;
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
    public static int NumberOfMeetings;
    public static void ClearAndReload()
    {
        NumberOfMeetings = 0;
    }
}
