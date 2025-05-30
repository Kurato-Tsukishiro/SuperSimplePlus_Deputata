using HarmonyLib;

namespace SuperSimplePlus.Patches;

/// <summary>
/// チャットログに記載する、システムメッセージに関わるHarmonyPatchを纏めている。
/// </summary>
[HarmonyPatch]
class AllHarmonyPatch
{
#pragma warning disable 8321
    // HarmonyPatchはローカル宣言で呼び出していなくても動くのに「ローカル関数 '関数名' は宣言されていますが、一度も使用されていません」と警告が出る為
    // このメソッドでは警告を表示しないようにしている

    private static int LastPost_was;
    private static bool isValidChatLog => SSPPlugin.ChatLog.Value;

    static void ChatLogHarmony()
    {
        // チャット履歴の保存
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat)), HarmonyPrefix]
        static void AddChatPrefix(PlayerControl sourcePlayer, string chatText)
        {
            if (!isValidChatLog) return;
            RecordingChatPatch.MonitorChat(sourcePlayer, chatText);
        }

        // チャットコマンドの監視
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat)), HarmonyPrefix]
        static bool SendChatPrefix(ChatController __instance)
        {
            if (!isValidChatLog) return true;

            RecordingChatPatch.SendChatPrefix(__instance, out bool handled);

            if (handled)
            {
                __instance.freeChatField.textArea.Clear();
                FastDestroyableSingleton<HudManager>.Instance.Chat.timeSinceLastMessage = 0f;
            }
            return !handled;
        }
    }

    /// <summary>ゲームログの作成関連で使用している HarmonyPatch</summary>
    static void GameLogHarmony()
    {
        // ゲーム開始時に情報を記載する
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin)), HarmonyPostfix]
        static void IntroCutsceneCoBeginPostfix()
        {
            if (!isValidChatLog) return;
            GameSystemLogPatch.IntroCutsceneCoBeginSystemLog();
        }

        // 会議開始
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start)), HarmonyPostfix]
        static void MeetingStartPostfix(MeetingHud __instance)
        {
            if (!isValidChatLog) return;
            GameSystemLogPatch.MeetingStartSystemLog(__instance);
        }

        // 死体通報
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody)), HarmonyPostfix]
        static void ReportDeadBodyPostfix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
        {
            if (!isValidChatLog) return;
            GameSystemLogPatch.ReportDeadBodySystemLog(__instance, target);
        }

        // 投票感知&記載(Hostのみ)
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote)), HarmonyPostfix]
        static void MeetingCastVotePostfix(byte srcPlayerId, byte suspectPlayerId)
        {
            if (!isValidChatLog) return;
            GameSystemLogPatch.MeetingCastVoteSystemLog(srcPlayerId, suspectPlayerId);
        }
        // 開票(Hostのみ)
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting)), HarmonyPostfix]
        static void CheckForEndVotingPostfix(MeetingHud __instance)
        {
            if (!isValidChatLog) return;
            GameSystemLogPatch.VoteLogMethodManager.MeetingCastVoteSave(__instance);
        }
        // 会議終了(airship以外)
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp)), HarmonyPostfix]
        static void MeetingEndPostfix(ExileController __instance)
        {
            if (!isValidChatLog) return;
            GameSystemLogPatch.DescribeMeetingEndSystemLog(__instance.initData?.networkedPlayer);
        }
        // 会議終了(airship)
        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn)), HarmonyPostfix]
        static void AirshipMeetingEndPostfix(AirshipExileController._WrapUpAndSpawn_d__11 __instance)
        {
            if (!isValidChatLog) return;

            int currentPost = __instance.__4__this.GetInstanceID();
            if (LastPost_was == currentPost) return;
            LastPost_was = currentPost;

            GameSystemLogPatch.DescribeMeetingEndSystemLog(__instance.__4__this.initData?.networkedPlayer);
        }

        // キル発生時
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer)), HarmonyPostfix]
        static void MurderPlayerPostfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!isValidChatLog) return;
            GameSystemLogPatch.MurderPlayerSystemLog(__instance, target);
        }
        // 試合終了
        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp)), HarmonyPostfix]
        static void EndGamePostfix()
        {
            if (!isValidChatLog) return;
            GameSystemLogPatch.EndGameSystemLog();
        }
    }
#pragma warning restore 8321
}
