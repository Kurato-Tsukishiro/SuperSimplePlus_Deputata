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
                    switch (p.PlatformData.Platform)
                    {
                        case Platforms.StandaloneEpicPC:
                        case Platforms.StandaloneSteamPC:
                            break;
                        case Platforms.Unknown:
                        case Platforms.StandaloneMac:
                        case Platforms.StandaloneWin10:
                        case Platforms.StandaloneItch:
                        case Platforms.IPhone:
                        case Platforms.Android:
                        case Platforms.Switch:
                        case Platforms.Xbox:
                        case Platforms.Playstation:
                            AmongUsClient.Instance.KickPlayer(p.Id, false);
                            Logger.Info($"Kick PlayerName:{p.PlayerName}({p.PlatformData.Platform})", "NotPCKick");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
