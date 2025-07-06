//参考 => https://github.com/Eisbison/TheOtherRoles/blob/main/TheOtherRoles/Patches/ClientOptionsPatch.cs
//参考 => https://github.com/ykundesu/SuperNewRoles/blob/1.8.1.0/SuperNewRoles/Patches/ClientOptionsPatch/ModOptions.cs
using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace SuperSimplePlus.Patches;

[HarmonyPatch]
internal static class ClientOptionsPatch
{
    // ChatLog を index = 3 として ボタン翻訳を上書きしているため, クライアント設定を変更した場合そこの対応も行う
    private static readonly SelectionBehaviour[] AllOptions = {
            new("NotPCKick", () => SSPPlugin.NotPCKick.Value = !SSPPlugin.NotPCKick.Value, SSPPlugin.NotPCKick.Value),
            new("NotPCBan", () => SSPPlugin.NotPCBan.Value = !SSPPlugin.NotPCBan.Value, SSPPlugin.NotPCBan.Value),
            new("FriendCodeBan", () => SSPPlugin.FriendCodeBan.Value = !SSPPlugin.FriendCodeBan.Value, SSPPlugin.FriendCodeBan.Value),
            new("ChatLog", () => SSPPlugin.ChatLog.Value = !SSPPlugin.ChatLog.Value, SSPPlugin.ChatLog.Value),
            new("HideFriendCode", () => SSPPlugin.HideFriendCode.Value = !SSPPlugin.HideFriendCode.Value, SSPPlugin.HideFriendCode.Value),
    };

    private static GameObject popUp;
    private static TextMeshPro titleText;

    private static ToggleButtonBehaviour modOption;
    private static List<ToggleButtonBehaviour> modButtons;
    private static TextMeshPro titleTextTitle;

    private static ToggleButtonBehaviour buttonPrefab;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    internal static void MainMenuManager_StartPostfix(MainMenuManager __instance)
    {
        // Prefab for the title
        var go = new GameObject("TitleTextSSP");
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.fontSize = 3;
        tmp.alignment = TextAlignmentOptions.Center;
        titleText = Object.Instantiate(tmp);
        titleText.gameObject.SetActive(false);
        Object.DontDestroyOnLoad(titleText);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    internal static void OptionsMenuBehaviour_StartPostfix(OptionsMenuBehaviour __instance)
    {
        if (!__instance.CensorChatButton) return;

        if (!popUp)
        {
            CreateCustom(__instance);
        }

        SetUpOptions();
        InitializeMoreButton(__instance);
    }

    private static void CreateCustom(OptionsMenuBehaviour prefab)
    {
        popUp = Object.Instantiate(prefab.gameObject);
        Object.DontDestroyOnLoad(popUp);
        var transform = popUp.transform;
        var pos = transform.localPosition;
        pos.z = -810f;
        transform.localPosition = pos;

        Object.Destroy(popUp.GetComponent<OptionsMenuBehaviour>());
        foreach (var gObj in popUp.gameObject.GetAllChilds())
        {
            if (gObj.name is not "Background" and not "CloseButton")
                Object.Destroy(gObj);
        }
        popUp.SetActive(false);
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static void HudManager_StartPostfix()
    {
        buttonPrefab = Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.transform.FindChild("Menu").GetComponent<OptionsMenuBehaviour>().CensorChatButton);
        Object.DontDestroyOnLoad(buttonPrefab);
        buttonPrefab.name = "CensorChatPrefab";
        buttonPrefab.gameObject.SetActive(false);
    }

    private static void InitializeMoreButton(OptionsMenuBehaviour __instance)
    {
        try
        {
            modOption = Object.Instantiate(buttonPrefab, __instance.CensorChatButton.transform.parent);
            Vector3 originPosition = new(-1.4f, -0.5f, -1.0f);
            modOption.transform.localPosition = originPosition + new Vector3(3.7f, 3.25f, 0.0f);
            var trans = modOption.transform.localPosition;
            modOption.gameObject.SetActive(true);
            trans = modOption.transform.position;
            var buttonSpriteRenderer = modOption.gameObject.AddComponent<SpriteRenderer>();
            buttonSpriteRenderer.sprite = Helpers.loadSpriteFromResources("SuperSimplePlus.Resources.SettingButton.png", 115f);
            modOption.Text.text = "<color=#FFFFFF00> </color>";
            var modOptionButton = modOption.GetComponent<PassiveButton>();

            var backColor = new Color(1f, 1f, 1f);
            var backSize = new Vector2(0.74f, 0.74f);

            modOption.Background.color = backColor;
            modOption.Background.size = backSize;

            modOptionButton.OnMouseOver = new UnityEvent();
            modOptionButton.OnMouseOver.AddListener((Action)(() =>
            {
                modOption.Background.color = new Color32(150, 81, 77, byte.MaxValue);
                modOption.Background.size = backSize;
            }));

            modOptionButton.OnMouseOut = new UnityEvent();
            modOptionButton.OnMouseOut.AddListener((Action)(() =>
            {
                modOption.Background.color = backColor;
                modOption.Background.size = backSize;
            }));

            modOptionButton.OnClick = new ButtonClickedEvent();

            modOptionButton.OnClick.AddListener((Action)(() =>
                {
                    if (!popUp) return;
                    if (__instance.transform.parent && __instance.transform.parent == FastDestroyableSingleton<HudManager>.Instance.transform)
                    {
                        popUp.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                        popUp.transform.localPosition = new Vector3(0, 0, -920f);
                    }
                    else
                    {
                        popUp.transform.SetParent(null);
                        Object.DontDestroyOnLoad(popUp);
                    }
                    CheckSetTitle();
                    RefreshOpen();
                }));
        }
        catch (Exception e)
        {
            Logger.Error($"SSPの設定ボタンの表示に失敗しました。 Error : {e}");
        }
    }

    private static void RefreshOpen()
    {
        popUp.gameObject.SetActive(false);
        popUp.gameObject.SetActive(true);
        SetUpOptions();
    }

    private static void CheckSetTitle()
    {
        if (!popUp || popUp.GetComponentInChildren<TextMeshPro>() || !titleText) return;

        var title = titleTextTitle = Object.Instantiate(titleText, popUp.transform);
        title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
        title.gameObject.SetActive(true);
        title.text = ModTranslation.GetString("SSPSettings");
        title.name = "TitleText";

        SetUpOptions();
    }

    private static void SetUpOptions()
    {
        if (popUp.transform.GetComponentInChildren<ToggleButtonBehaviour>()) return;

        modButtons = new List<ToggleButtonBehaviour>();

        for (var i = 0; i < AllOptions.Length; i++)
        {
            var info = AllOptions[i];

            var button = Object.Instantiate(buttonPrefab, popUp.transform);
            var pos = new Vector3(i % 2 == 0 ? -1.17f : 1.17f, 1.3f - i / 2 * 0.8f, -.5f);

            var transform = button.transform;
            transform.localPosition = pos;

            button.onState = info.DefaultValue;
            button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;

            button.Text.text = ModTranslation.GetString(info.Title);
            if (i == 3) { button.Text.text += GameLogManager.IsValidChatLog ? $"\n{ModTranslation.GetString("ChatLogOn")}" : $"\n{ModTranslation.GetString("ChatLogOff")}"; }
            button.Text.fontSizeMin = button.Text.fontSizeMax = 2.2f;
            button.Text.font = Object.Instantiate(titleText.font);
            button.Text.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 2);

            button.name = info.Title.Replace(" ", "") + "Toggle";
            button.gameObject.SetActive(true);

            var passiveButton = button.GetComponent<PassiveButton>();
            var colliderButton = button.GetComponent<BoxCollider2D>();

            colliderButton.size = new Vector2(2.2f, .7f);

            passiveButton.OnClick = new ButtonClickedEvent();
            passiveButton.OnMouseOut = new UnityEvent();
            passiveButton.OnMouseOver = new UnityEvent();

            passiveButton.OnClick.AddListener((Action)(() =>
            {
                button.onState = info.OnClick();
                button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
            }));

            passiveButton.OnMouseOver.AddListener((Action)(() => button.Background.color = new Color32(34, 139, 34, byte.MaxValue)));
            passiveButton.OnMouseOut.AddListener((Action)(() => button.Background.color = button.onState ? Color.green : Palette.ImpostorRed));

            foreach (var spr in button.gameObject.GetComponentsInChildren<SpriteRenderer>())
                spr.size = new Vector2(2.2f, .7f);
            modButtons.Add(button);
        }
    }
    private static IEnumerable<GameObject> GetAllChilds(this GameObject Go)
    {
        for (var i = 0; i < Go.transform.childCount; i++)
        {
            yield return Go.transform.GetChild(i).gameObject;
        }
    }

    internal class SelectionBehaviour
    {
        /// <summary>翻訳キー</summary>
        internal readonly string Key;
        /// <summary>表示名</summary>
        internal readonly string Title;
        internal readonly Func<bool> OnClick;
        internal readonly bool DefaultValue;

        internal SelectionBehaviour(string key, Func<bool> onClick, bool defaultValue)
        {
            Key = key;
            Title = ModTranslation.GetString(key);
            OnClick = onClick;
            DefaultValue = defaultValue;
        }
    }
}
