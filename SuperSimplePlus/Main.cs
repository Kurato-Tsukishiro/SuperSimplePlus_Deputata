//引用=>https://github.com/reitou-mugicha/TownOfSuper/blob/main/TownOfSuper/Main.cs
using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace SuperSimplePlus;
[BepInPlugin(Id, "SuperSimplePlus", Version)]
public class SSPPlugin : BasePlugin
{
    public const String Id = "jp.Kurato-Tsukishiro.SuperSimplePlus_Deputata";
    public const String Version = "2.3.0.0";

    public const String ColoredModName = "<color=#96514d>SSP_Deputata</color>";
    public const String shortModName = "SSP_Deputata";

    public static ConfigEntry<bool> debugTool { get; set; }
    public static ConfigEntry<bool> NotPCKick { get; set; }
    public static ConfigEntry<bool> NotPCBan { get; set; }
    public static ConfigEntry<bool> GameLog { get; set; }
    public static ConfigEntry<bool> HideFriendCode { get; set; }
    public static ConfigEntry<bool> FriendCodeBan { get; set; }

    public Harmony Harmony = new(Id);
    internal static BepInEx.Logging.ManualLogSource Logger;

    public static ConfigEntry<bool> DebugMode { get; private set; }

    public override void Load()
    {
        Logger = Log;

        SuperSimplePlus.Logger.Info("SuperSimplePlus_Deputata 読み込み開始", shortModName);

        debugTool = Config.Bind("Client Options", "Debug Tool", false);
        NotPCKick = Config.Bind("Client Options", "NotPCKick", false, "Steam及びEpic以外のプラットフォームのプレイヤーをキックするか");
        NotPCBan = Config.Bind("Client Options", "NotPCBan", false, "Steam及びEpic以外のプラットフォームのプレイヤーをバンするか (優先)");
        GameLog = Config.Bind("Client Options", "GameLog", false, "ゲームログ及びチャットログを作成するか");
        HideFriendCode = Config.Bind("Client Options", "HideFriendCode", true, "ゲームログでフレンドコードを非表示にするか");
        FriendCodeBan = Config.Bind("Client Options", "FriendCodeBan", false, "外部ファイルに記録されたフレンドコードをバンする機能を有効にするか");

        //Load
        ModTranslation.LoadCsv();
        ImmigrationCheck.LoadFriendCodeList();

        Harmony.PatchAll();

        Patches.GameLogManager.Load();

        SuperSimplePlus.Logger.Info("SuperSimplePlus_Deputata 読み込み終了", shortModName);
    }
}

[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
public class ShowModStampPatch
{
    public static void Postfix(ModManager __instance) => __instance.ShowModStamp();
}

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public class VersionShowerPatch
{
    public static void Postfix(VersionShower __instance) => __instance.text.text += $" + {SSPPlugin.ColoredModName} ver." + SSPPlugin.Version; //<color=#ffddef>AZ</color>
}
