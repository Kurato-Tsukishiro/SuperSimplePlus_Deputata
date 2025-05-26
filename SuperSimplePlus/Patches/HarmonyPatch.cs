using HarmonyLib;

namespace SuperSimplePlus.Patches;

/// <summary>
/// チャットログに記載する、システムメッセージに関わるHarmonyPatchを纏めている。
/// </summary>
[HarmonyPatch]
class ChatLogHarmonyPatch
{
#pragma warning disable 8321
    // HarmonyPatchはローカル宣言で呼び出していなくても動くのに「ローカル関数 '関数名' は宣言されていますが、一度も使用されていません」と警告が出る為
    // このメソッドでは警告を表示しないようにしている

    private static int LastPost_was;

    public static void ChatLogHarmony()
    {
        if (!SSPPlugin.ChatLog.Value) return; // ChatLogを作成しない設定だったら読まないようにする。

        // ゲーム開始時に情報を記載する
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin)), HarmonyPostfix]
        static void IntroCutsceneCoBeginPostfix() => SystemLogMethodManager.IntroCutsceneCoBeginSystemLog();

        // 会議開始
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start)), HarmonyPostfix]
        static void MeetingStartPostfix(MeetingHud __instance) => SystemLogMethodManager.MeetingStartSystemLog(__instance);

        // 死体通報
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody)), HarmonyPostfix]
        static void ReportDeadBodyPostfix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target) => SystemLogMethodManager.ReportDeadBodySystemLog(__instance, target);

        // 投票感知&記載(Hostのみ)
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote)), HarmonyPostfix]
        static void MeetingCastVotePostfix(byte srcPlayerId, byte suspectPlayerId) => SystemLogMethodManager.MeetingCastVoteSystemLog(srcPlayerId, suspectPlayerId);

        // 開票(Hostのみ)
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting)), HarmonyPostfix]
        static void CheckForEndVotingPostfix(MeetingHud __instance) => SystemLogMethodManager.VoteLogMethodManager.MeetingCastVoteSave(__instance);

        // 会議終了(airship以外)
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp)), HarmonyPostfix]
        static void MeetingEndPostfix(ExileController __instance) => SystemLogMethodManager.DescribeMeetingEndSystemLog(__instance.initData?.networkedPlayer);

        // 会議終了(airship)
        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn)), HarmonyPostfix]
        static void AirshipMeetingEndPostfix(AirshipExileController._WrapUpAndSpawn_d__11 __instance)
        {
            int currentPost = __instance.__4__this.GetInstanceID();
            if (LastPost_was == currentPost) return;
            LastPost_was = currentPost;

            SystemLogMethodManager.DescribeMeetingEndSystemLog(__instance.__4__this.initData?.networkedPlayer);
        }

        // キル発生時
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer)), HarmonyPostfix]
        static void MurderPlayerPostfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target) => SystemLogMethodManager.MurderPlayerSystemLog(__instance, target);

        // 試合終了
        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp)), HarmonyPostfix]
        static void EndGamePostfix() => SystemLogMethodManager.EndGameSystemLog();
    }
#pragma warning restore 8321
}
