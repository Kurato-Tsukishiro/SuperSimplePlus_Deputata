using HarmonyLib;
using UnityEngine;

namespace SuperSimplePlus.Patches
{
    [HarmonyPatch]
    public class UpdatePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static void HudManager_UpdatePostfix()
        {
            if (ClientOptionsPatch.SSPOptionsMenu)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    GameObject.Destroy(ClientOptionsPatch.SSPOptionsMenu);
                }
            }
        }
    }
}
