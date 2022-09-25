//引用=>https://github.com/reitou-mugicha/TownOfSuper/blob/main/TownOfSuper/Main.cs
using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;

namespace SuperSimplePlus
{
    [BepInPlugin(Id, "SuperSimplePlus", Version)]
    public class SSPPlugin : BasePlugin
    {
        public const String Id = "jp.satsumaimoamo.SuperSimplePlus";
        public const String Version = "1.3.0";

        public const String ColoredModName = "<color=#8cfc03>SuperSimplePlus</color>";

        public static ConfigEntry<bool> debugTool { get; set; }
        public static ConfigEntry<string> StereotypedText { get; set; }
        public static ConfigEntry<bool> NotPCKick { get; set; }
        public static ConfigEntry<bool> NotPCBan { get; set; }

        public Harmony Harmony = new(Id);
        internal static BepInEx.Logging.ManualLogSource Logger;

        public static ConfigEntry<bool> DebugMode { get; private set; }

        public override void Load()
        {
            Logger = Log;

            SuperSimplePlus.Logger.Info("SuperSimplePlusLoading!!!!!!!!!!!!!!!!!", "SuperSimplePlus");

            debugTool = Config.Bind("Client Options", "Debug Tool", false);
            NotPCKick = Config.Bind("Client Options", "NotPCKick", false);
            NotPCBan = Config.Bind("Client Options", "NotPCBan", false);
            StereotypedText = Config.Bind("Client Options", "StereotypedText", "SuperSimplePlus定型文");

            //Load
            ModTranslation.Load();

            Harmony.PatchAll();

            SuperSimplePlus.Logger.Info("SuperSimplePlus End of loading!!!!!!!!!!!!!!!!!", "SuperSimplePlus");
        }
    }

    [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
    public class ShowModStampPatch
    {
        public static void Postfix(ModManager __instance)
        {
            __instance.ShowModStamp();
        }
    }

    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public class VersionShowerPatch
    {
        public static void Postfix(VersionShower __instance)
        {
            __instance.text.text += $" + {SSPPlugin.ColoredModName} ver." + SSPPlugin.Version; //<color=#ffddef>AZ</color>
        }
    }
}
