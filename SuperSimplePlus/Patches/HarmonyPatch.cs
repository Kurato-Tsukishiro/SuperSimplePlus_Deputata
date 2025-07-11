using System.Linq;
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

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.MakePublic))]
    class MakePublicPatch
    {
        /// <summary>Modを併用しているか</summary>
        /// <value>null => cache無し / true => 併用している / false => 単独導入</value>
        internal static bool? cachedHasOtherMods { get; private set; } = null;

        /// <summary>公開部屋への変更が可能か?</summary>
        /// <param name="__instance"></param>
        /// <returns>true => 可能 / false => 不可能</returns>
        internal static bool Prefix(GameStartManager __instance)
        {
            if (!AmongUsClient.Instance.AmHost || Helpers.IsCustomServer()) return true;

            // 参考 => https://g.co/gemini/share/145248f63766
            if (cachedHasOtherMods == null) // checkを一度も行っていない時のみ実行
            {
                var patchedMethods = Harmony.GetAllPatchedMethods();

                // 適用されたパッチが1つでも存在するかをチェック
                var otherOwners = patchedMethods
                                    .Select(Harmony.GetPatchInfo)
                                    .Where(patchInfo => patchInfo != null)
                                    .SelectMany(patchInfo => patchInfo.Owners)
                                    .Distinct()
                                    .Where(IsPluginOwnerValid)
                                    .ToList();

                cachedHasOtherMods = otherOwners.Any();

                if (cachedHasOtherMods == true)
                {
                    Logger.Info("SSP_Dは他のMODと併用されています。");
                    foreach (var owner in otherOwners) { Logger.Info($"併用MODの可能性: {owner}"); }
                }
                else
                {
                    Logger.Info("SSP_D 単独導入か、他のMODがHarmonyを使用していません。");
                }
            }

            // 併用状態の場合、公開部屋を可能と判定する。 (単独導入の場合、公開部屋を不可能にする)
            // 此処でtrueを返しても、併用しているmodがfalseを返したならそちらが優先される。
            var hasOtherMods = cachedHasOtherMods == true;

            if (!hasOtherMods)
            {
                if (FastDestroyableSingleton<HudManager>.Instance != null && FastDestroyableSingleton<HudManager>.Instance.Chat != null)
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, ModTranslation.GetString("MakePublicError"));
            }

            return hasOtherMods;

        static bool IsPluginOwnerValid(string owner)
        {
            // 自分自身のGuid(SSPPlugin.Id)と、Harmonyの自動生成IDを除外
            return owner != SSPPlugin.Id && !owner.StartsWith("harmony-auto-");
        }
        }
    }
}
