using System.Linq;
using HarmonyLib;
using InnerNet;
using SuperSimplePlus.Modules;

namespace SuperSimplePlus.Patches;

/// <summary>
/// SSP_Dで追加した機能関連のHarmonyPatchを纏めている
/// </summary>
[HarmonyPatch]
class AllHarmonyPatch
{
    private static readonly bool IsValidChatLog = ClientOptionsPatch.StartupState["UseSSPDFeature"] && ClientOptionsPatch.StartupState["GameLog"];

    /// <summary>チャットログ関連のHarmonyPatch</summary>
    [HarmonyPatch]
    static class ChatLogHarmony
    {
        /// <summary>チャット履歴の保存</summary>
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        static class AddChatPatch
        {
            static bool Prepare() => IsValidChatLog;
            static void Prefix(PlayerControl sourcePlayer, string chatText) => RecordingChatPatch.MonitorChat(sourcePlayer, chatText);
        }

        /// <summary>コマンドの監視</summary>
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        static class SendChatPatch
        {
            static bool Prepare() => IsValidChatLog;
            static bool Prefix(ChatController __instance)
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
    }

    /// <summary>ゲームログの作成関連で使用しているHarmonyPatch</summary>
    [HarmonyPatch]
    static class GameLogHarmony
    {
        /// <summary>ゲーム開始時に情報を記載する</summary>
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
        static class CoBeginPatch
        {
            static bool Prepare() => IsValidChatLog;
            static void Postfix() => GameSystemLogPatch.IntroCutsceneCoBeginSystemLog();
        }

        /// <summary>会議開始時に情報を記載する</summary>
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        static class MeetingHudStartPatch
        {
            static bool Prepare() => IsValidChatLog;
            static void Postfix(MeetingHud __instance) => GameSystemLogPatch.MeetingStartSystemLog(__instance);
        }

        /// <summary>死体通報時に情報を記載する</summary>
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
        static class ReportDeadBodyPatch
        {
            static bool Prepare() => IsValidChatLog;
            static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target) => GameSystemLogPatch.ReportDeadBodySystemLog(__instance, target);
        }

        /// <summary>投票の感知と記載を行う(Hostのみ)</summary>
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote)),]
        static class CastVotePatch
        {
            static bool Prepare() => IsValidChatLog;
            static void Postfix(byte srcPlayerId, byte suspectPlayerId) => GameSystemLogPatch.MeetingCastVoteSystemLog(srcPlayerId, suspectPlayerId);
        }

        /// <summary>開票時に情報を記載する</summary>
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        internal class CheckForEndVotingPatch
        {
            static bool Prepare() => IsValidChatLog;
            static void Postfix(MeetingHud __instance) => GameSystemLogPatch.VoteLogMethodManager.MeetingCastVoteSave(__instance);
        }

        /// <summary>会議終了時(airship以外)に情報を記載する</summary>
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        static class ExileControllerWrapUpPatch
        {
            static bool Prepare() => IsValidChatLog;
            static void Postfix(ExileController __instance) => GameSystemLogPatch.DescribeMeetingEndSystemLog(__instance.initData?.networkedPlayer);
        }

        /// <summary>会議終了時(airship)に情報を記載する</summary>
        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        static class AirshipExileControllerWrapUpAndSpawnPatch
        {
            static int LastPost_was;

            static bool Prepare() => IsValidChatLog;
            static void Postfix(AirshipExileController._WrapUpAndSpawn_d__11 __instance)
            {

                int currentPost = __instance.__4__this.GetInstanceID();
                if (LastPost_was == currentPost) return;
                LastPost_was = currentPost;

                GameSystemLogPatch.DescribeMeetingEndSystemLog(__instance.__4__this.initData?.networkedPlayer);
            }
        }

        /// <summary>キル発生時に情報を記載する</summary>
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        static class MurderPlayerPatch
        {
            static bool Prepare() => IsValidChatLog;
            static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target) => GameSystemLogPatch.MurderPlayerSystemLog(__instance, target);
        }

        /// <summary>試合終了時に情報を記載する</summary>
        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
        static class SetEverythingUpPatch
        {
            static bool Prepare() => IsValidChatLog;
            static void Postfix() => GameSystemLogPatch.EndGameSystemLog();
        }
    }

    /// <summary>FriendCodeを使用した入退出管理関連のHarmonyPatch</summary>
    [HarmonyPatch]
    static class FriendCodeImmigrationHarmony
    {

        /// <summary>自身が入室した時に、既入室者の情報の取得を行う処理</summary>
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
        static class OnGameJoinedPatch
        {
            internal static bool Prepare() => ClientOptionsPatch.StartupState["UseSSPDFeature"];
            internal static void Postfix(AmongUsClient __instance) => FriendCodeImmigrationPatch.RecordsOfExistingPlayer();
        }

        /// <summary>誰かが入室した時に記録及び入室管理を行う処理</summary>
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        static class OnPlayerJoinedPatch
        {
            internal static bool Prepare() => ClientOptionsPatch.StartupState["UseSSPDFeature"];
            internal static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client) => FriendCodeImmigrationPatch.OnPlayerJoined_postfix(client);
        }


        /// <summary>誰かが退出した時に記録を行う処理</summary>
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
        static class OnPlayerLeftPatch
        {
            internal static bool Prepare() => ClientOptionsPatch.StartupState["UseSSPDFeature"];
            //参考=>https://github.com/haoming37/TheOtherRoles-GM-Haoming/blob/haoming-main/TheOtherRoles/Patches/GameStartManagerPatch.cs
            public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client, [HarmonyArgument(1)] DisconnectReasons reason)
                => ImmigrationCheck.WriteBunReport(client, reason);
        }

        /// <summary>コマンドの管理</summary>
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        static class SendChatPatch
        {
            internal static bool Prepare() => ClientOptionsPatch.StartupState["UseSSPDFeature"];
            static bool Prefix(ChatController __instance)
            {
                FriendCodeImmigrationPatch.ChatCommand(__instance, out bool handled);

                if (handled)
                {
                    __instance.freeChatField.textArea.Clear();
                    FastDestroyableSingleton<HudManager>.Instance.Chat.timeSinceLastMessage = 0f;
                }
                return !handled;
            }
        }
    }

    /// <summary>部屋の作成関連のHarmonyPatch</summary>
    [HarmonyPatch]
    static class RoomControlHarmony
    {
        /// <summary>Modを併用しているか</summary>
        /// <value>null => cache無し / true => 併用している / false => 単独導入</value>
        internal static bool? cachedHasOtherMods { get; private set; } = null;

        /// <summary>SSP_D以外が有しているHarmonyPatchか判定する</summary>
        /// <param name="owner">Guid</param>
        /// <returns>true => 他のmod / false => 自身 </returns>
        static bool IsPluginOwnerValid(string owner)
        {
            // 自分自身のGuid(SSPPlugin.Id)と、Harmonyの自動生成IDを除外
            return owner != SSPPlugin.Id && !owner.StartsWith("harmony-auto-");
        }

        /// <summary>公開部屋の制御を行う</summary>
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.MakePublic))]
        static class MakePublicPatch
        {
            static bool Prepare() => ClientOptionsPatch.StartupState["UseSSPDFeature"];

            /// <summary>公開部屋への変更が可能か?</summary>
            /// <param name="__instance"></param>
            /// <returns>true => 可能 / false => 不可能</returns>
            static bool Prefix(GameStartManager __instance)
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
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        static class GameStartManagerUpdatePatch
        {
            static bool Prepare() => ClientOptionsPatch.StartupState["UseSSPDFeature"];
            //参考=>https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Patches/ShareGameVersionPatch.cs
            /// <summary>1人以上からゲームを開始できるようにする</summary>
            /// <param name="__instance"></param>
            static void Prefix(GameStartManager __instance) => __instance.MinPlayers = 1;
        }
    }
}
