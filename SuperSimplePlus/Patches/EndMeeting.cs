using HarmonyLib;
using UnityEngine;

namespace SuperSimplePlus.Patches;
[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
class ShoutcutKeyBoard
{
    public static void Postfix()
    {
        if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
        if (!AmongUsClient.Instance.AmHost) return;

        //廃村
        if (Helpers.GetManyKeyDown(new[] { KeyCode.A, KeyCode.LeftShift, KeyCode.RightShift }))//Aと右左シフトを押したとき
        {
            Logger.Info("================= SSPによる 廃村 ==================", "End Game");
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
        }

        //ミーティング強制終了
        if (Helpers.GetManyKeyDown(new[] { KeyCode.C, KeyCode.LeftShift, KeyCode.RightShift }))//Cと右左シフトを押したとき
        {
            Logger.Info("会議強制終了", "MeetingHud");
            if (MeetingHud.Instance != null)
                MeetingHud.Instance.RpcClose();//会議を爆破
        }
    }
}
