using HarmonyLib;

namespace SuperSimplePlus.Patches
{
    [HarmonyPatch]
    public class PlayerControlPatch
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        [HarmonyPostfix]
        public static void PlayerControl_MurderPlayerPostfixPatch(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (PlayerControl.GameOptions.MapId == 5 && target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                ClientOptionsPatch.SSPSettingButton.transform.localPosition = new(ClientOptionsPatch.SSPSettingButton.transform.localPosition.x, -0.25f, ClientOptionsPatch.SSPSettingButton.transform.localPosition.z);
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
        [HarmonyPostfix]
        public static void PlayerControl_ExildPlayerPostfixPatch(PlayerControl __instance)
        {
            if (PlayerControl.GameOptions.MapId == 5 && __instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                ClientOptionsPatch.SSPSettingButton.transform.localPosition = new(ClientOptionsPatch.SSPSettingButton.transform.localPosition.x, -0.25f, ClientOptionsPatch.SSPSettingButton.transform.localPosition.z);
            }
        }
    }
}
