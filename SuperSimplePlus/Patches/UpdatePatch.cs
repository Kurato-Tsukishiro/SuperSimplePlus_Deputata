using HarmonyLib;
using UnityEngine;

namespace SuperSimplePlus.Patches
{
    [HarmonyPatch]
    public class UpdatePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static void HudManager_UpdatePostfix(HudManager __instance)
        {
            if (ClientOptionsPatch.SSPOptionsMenu)
            {
                PlayerControl.LocalPlayer.moveable = false;
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    GameObject.Destroy(ClientOptionsPatch.SSPOptionsMenu);
                    PlayerControl.LocalPlayer.moveable = true;
                }
            }
        }
    }
}
