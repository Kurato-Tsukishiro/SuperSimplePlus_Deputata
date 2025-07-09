using HarmonyLib;

namespace SuperSimplePlus.Patches;

/// <summary>
/// チャットログに記載する、システムメッセージに関わるHarmonyPatchを纏めている。
/// </summary>
[HarmonyPatch]
class AllHarmonyPatch
{
    private static int LastPost_was;

    [HarmonyPatch]
    static class ChatLogHarmony
    {
        // チャット履歴の保存
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat)), HarmonyPrepare]
        static bool AddChatPrepare() => GameLogManager.IsValidChatLog;
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat)), HarmonyPrefix]
        static void AddChatPrefix(PlayerControl sourcePlayer, string chatText)
        {
            RecordingChatPatch.MonitorChat(sourcePlayer, chatText);
        }

        // チャットコマンドの監視
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat)), HarmonyPrepare]
        static bool SendChatPrepare() => GameLogManager.IsValidChatLog;
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat)), HarmonyPrefix]
        static bool SendChatPrefix(ChatController __instance)
        {
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
    [HarmonyPatch]
    static class GameLogHarmony
    {
        // ゲーム開始時に情報を記載する
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin)), HarmonyPrepare]
        static bool IntroCutsceneCoBeginPrepare() => GameLogManager.IsValidChatLog;
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin)), HarmonyPostfix]
        static void IntroCutsceneCoBeginPostfix()
        {
            GameSystemLogPatch.IntroCutsceneCoBeginSystemLog();
        }

        // 会議開始
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start)), HarmonyPrepare]
        static bool MeetingStartPrepare() => GameLogManager.IsValidChatLog;
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start)), HarmonyPostfix]
        static void MeetingStartPostfix(MeetingHud __instance)
        {
            GameSystemLogPatch.MeetingStartSystemLog(__instance);
        }

        // 死体通報
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody)), HarmonyPrepare]
        static bool ReportDeadBodyPrepare() => GameLogManager.IsValidChatLog;
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody)), HarmonyPostfix]
        static void ReportDeadBodyPostfix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
        {
            GameSystemLogPatch.ReportDeadBodySystemLog(__instance, target);
        }

        // 投票感知&記載(Hostのみ)
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote)), HarmonyPrepare]
        static bool MeetingCastVotePrepare() => GameLogManager.IsValidChatLog;
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote)), HarmonyPostfix]
        static void MeetingCastVotePostfix(byte srcPlayerId, byte suspectPlayerId)
        {
            GameSystemLogPatch.MeetingCastVoteSystemLog(srcPlayerId, suspectPlayerId);
        }
        // 開票(Hostのみ)
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting)), HarmonyPrepare]
        static bool CheckForEndVotingPrepare() => GameLogManager.IsValidChatLog;
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting)), HarmonyPostfix]
        static void CheckForEndVotingPostfix(MeetingHud __instance)
        {
            GameSystemLogPatch.VoteLogMethodManager.MeetingCastVoteSave(__instance);
        }
        // 会議終了(airship以外)
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp)), HarmonyPrepare]
        static bool MeetingEndPrepare() => GameLogManager.IsValidChatLog;
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp)), HarmonyPostfix]
        static void MeetingEndPostfix(ExileController __instance)
        {
            GameSystemLogPatch.DescribeMeetingEndSystemLog(__instance.initData?.networkedPlayer);
        }
        // 会議終了(airship)
        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn)), HarmonyPrepare]
        static bool AirshipMeetingEndPrepare() => GameLogManager.IsValidChatLog;
        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn)), HarmonyPostfix]
        static void AirshipMeetingEndPostfix(AirshipExileController._WrapUpAndSpawn_d__11 __instance)
        {

            int currentPost = __instance.__4__this.GetInstanceID();
            if (LastPost_was == currentPost) return;
            LastPost_was = currentPost;

            GameSystemLogPatch.DescribeMeetingEndSystemLog(__instance.__4__this.initData?.networkedPlayer);
        }

        // キル発生時
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer)), HarmonyPrepare]
        static bool MurderPlayerPrepare() => GameLogManager.IsValidChatLog;
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer)), HarmonyPostfix]
        static void MurderPlayerPostfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            GameSystemLogPatch.MurderPlayerSystemLog(__instance, target);
        }
        // 試合終了
        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp)), HarmonyPrepare]
        static bool EndGamePrepare() => GameLogManager.IsValidChatLog;
        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp)), HarmonyPostfix]
        static void EndGamePostfix()
        {
            GameSystemLogPatch.EndGameSystemLog();
        }
    }
}
