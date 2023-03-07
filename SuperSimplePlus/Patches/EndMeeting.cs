using HarmonyLib;
using UnityEngine;

namespace SuperSimplePlus.Patches;
[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
class ShoutcutKeyBoard
{
    public static void Postfix()
    {
        if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)//ゲームがスタートしているとき
        {
            if (AmongUsClient.Instance.AmHost)//ホストで
            {
                //廃村
                if (Input.GetKeyDown(KeyCode.A) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift))//Aと右左シフトを押したとき
                {
                    Logger.Info("================= SSPによる 廃村 ==================", "End Game");
                    GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                }
                //ミーティング強制終了
                if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift))//Cと右左シフトを押したとき
                {
                    Logger.Info("会議強制終了", "MeetingHud");
                    MeetingHud.Instance.RpcClose();//会議を爆破
                }
            }
        }
    }
}
