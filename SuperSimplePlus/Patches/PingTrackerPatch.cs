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
            __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
            __instance.text.text = ThisAssembly.Git.Branch == "main" ? $"{SSPPlugin.ColoredModName} ver.{SSPPlugin.Version}\n{__instance.text.text}" : $"{SSPPlugin.ColoredModName} ver.{SSPPlugin.Version}\n{ThisAssembly.Git.Branch}({ThisAssembly.Git.Commit})\n{__instance.text.text}";
            __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(1.2f, 0.1f, 0.5f);
        }
    }
}
