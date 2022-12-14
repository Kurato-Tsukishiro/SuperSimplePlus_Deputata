using InnerNet;
using System;
using HarmonyLib;
using SuperSimplePlus.Modules;

namespace SuperSimplePlus.Patches
{
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class GameStartManagerUpdatePatch
    {
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
    }
    //参考=>https://github.com/haoming37/TheOtherRoles-GM-Haoming/blob/haoming-main/TheOtherRoles/Patches/GameStartManagerPatch.cs
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    public class AmongUsClientOnPlayerLeftPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client, [HarmonyArgument(1)] DisconnectReasons reason)
        {
            Logger.Info($"PlayerName: \"{client.PlayerName}(ID:{client.Id})({client.PlatformData.Platform})\" Left (Reason: {reason})", "OnPlayerLeft");
        }
    }
}
