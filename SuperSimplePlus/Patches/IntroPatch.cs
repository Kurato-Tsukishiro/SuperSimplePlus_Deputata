using HarmonyLib;

namespace SuperSimplePlus.Patches
{
    [HarmonyPatch]
    public class IntroPatch
    {
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
        [HarmonyPostfix]
        public static void IntroOnDestroyPostfixPatch()
        {
            ClientOptionsPatch.SSPSettingButton.transform.localPosition = new(ClientOptionsPatch.SSPSettingButton.transform.localPosition.x, 0.55f, ClientOptionsPatch.SSPSettingButton.transform.localPosition.z);
        }
    }
}
