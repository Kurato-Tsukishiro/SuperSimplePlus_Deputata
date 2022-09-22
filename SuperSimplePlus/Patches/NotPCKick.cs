using System;
using HarmonyLib;
using SuperSimplePlus.Modules;

namespace SuperSimplePlus.Patches
{
    public class NotPCKick
    {
        public static bool IsKick = false;
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class GameStartManagerUpdatePatch
    {
        public static void Postfix(GameStartManager __instance)
        {
            if (AmongUsClient.Instance.AmHost && NotPCKick.IsKick)
            {
                foreach (InnerNet.ClientData p in AmongUsClient.Instance.allClients)
                {
                    if (p.PlatformData.Platform is not Platforms.StandaloneEpicPC and not Platforms.StandaloneSteamPC)
                    {
                        AmongUsClient.Instance.KickPlayer(p.Id, false);
                        Logger.Info($"Kick PlayerName:{p.PlayerName}({p.PlatformData.Platform})", "NotPCKick");
                    }
                }
            }
        }
    }
}
