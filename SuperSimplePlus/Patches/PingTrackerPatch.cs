using HarmonyLib;
using UnityEngine;
namespace SuperSimplePlus.Patches;
public class PingTrackerPatch
{
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public class PingTrackerUpdatePatch
    {
        public static void Postfix(PingTracker __instance)
        {
            var bc = $"{ThisAssembly.Git.Branch}({ThisAssembly.Git.Commit})";
            __instance.text.text =
                AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started
                    ? $"{__instance.text.text}<size=67%><line-height=55%>\n<pos=25%>{SSPPlugin.ColoredModName} ver.{SSPPlugin.Version}{(ThisAssembly.Git.Branch == "main" ? "" : " (β)")}</size></line-height>"
                    : ThisAssembly.Git.Branch == "main" ?
                        $"{__instance.text.text}\n{SSPPlugin.ColoredModName} ver.{SSPPlugin.Version}" :
                        $"{__instance.text.text}\n{SSPPlugin.ColoredModName} ver.{SSPPlugin.Version}\n{bc}";
        }
    }
}
