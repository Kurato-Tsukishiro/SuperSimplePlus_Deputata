//参考=>https://github.com/Eisbison/TheOtherRoles/blob/main/TheOtherRoles/Patches/ClientOptionsPatch.cs
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
public static class ClientOptionsPatch
{
    private static readonly SelectionBehaviour[] AllOptions =
        {
            new(ModTranslation.GetString("NotPCKick"),()=> SSPPlugin.NotPCKick.Value = !SSPPlugin.NotPCKick.Value,SSPPlugin.NotPCKick.Value),
            new(ModTranslation.GetString("NotPCBan"),()=> SSPPlugin.NotPCBan.Value = !SSPPlugin.NotPCBan.Value,SSPPlugin.NotPCBan.Value),
            new(ModTranslation.GetString("ChatLog"),()=> SSPPlugin.ChatLog.Value = !SSPPlugin.ChatLog.Value,SSPPlugin.ChatLog.Value),
            new(ModTranslation.GetString("DisplayFriendCode"),()=> SSPPlugin.DisplayFriendCode.Value = !SSPPlugin.DisplayFriendCode.Value,SSPPlugin.DisplayFriendCode.Value),
            new(ModTranslation.GetString("FriendCodeBan"),()=> SSPPlugin.FriendCodeBan.Value = !SSPPlugin.FriendCodeBan.Value,SSPPlugin.FriendCodeBan.Value),
        };

    public static PassiveButton SSPSettingButton;
    public static SpriteRenderer SSPSettingSpriteRenderer;
    public static GameObject SSPOptionsMenu;
    private static TextMeshPro titleText;

    private static ToggleButtonBehaviour buttonPrefab;


    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static void MainMenuManager_StartPostfix(MainMenuManager __instance)
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
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static void HudManager_StartPostfix()
    {
        SSPSettingButton = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.MapButton, HudManager.Instance.transform.FindChild("Buttons").FindChild("TopRight"));

        SSPSettingSpriteRenderer = SSPSettingButton.GetComponent<SpriteRenderer>();

        SSPSettingSpriteRenderer.sprite = Helpers.loadSpriteFromResources("SuperSimplePlus.Resources.SettingButton.png", 115f);

        SSPSettingSpriteRenderer.gameObject.SetActive(true);
        SSPSettingSpriteRenderer.enabled = true;

        SSPSettingButton.OnClick = new ButtonClickedEvent();

        SSPSettingButton.OnClick.AddListener((UnityAction)(() => { SSPSettingButtonOnClick(); }));

        buttonPrefab = Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.transform.FindChild("Menu").GetComponent<OptionsMenuBehaviour>().CensorChatButton);
        Object.DontDestroyOnLoad(buttonPrefab);
        buttonPrefab.name = "CensorChatPrefab";
        buttonPrefab.gameObject.SetActive(false);

        SSPSettingSpriteRenderer.gameObject.transform.localPosition = new(SSPSettingButton.transform.localPosition.x, SSPSettingButton.transform.localPosition.y - 0.75f, SSPSettingButton.transform.localPosition.z);
    }

    private static void SSPSettingButtonOnClick()
    {
        if (SSPOptionsMenu) return;

        var __instance = FastDestroyableSingleton<HudManager>.Instance;

        SSPOptionsMenu = Object.Instantiate(__instance.transform.FindChild("Menu").GetComponent<OptionsMenuBehaviour>().Background.gameObject);
        SSPOptionsMenu.transform.SetParent(__instance.transform);
        SSPOptionsMenu.transform.position = __instance.transform.FindChild("Menu").GetComponent<OptionsMenuBehaviour>().transform.position;
        SSPOptionsMenu.transform.localScale = new(SSPOptionsMenu.transform.localScale.x * 0.9f, SSPOptionsMenu.transform.localScale.y * 0.9f, 0f);

        PassiveButton closeButton = GameObject.Instantiate(SSPSettingButton, SSPSettingButton.transform);

        SpriteRenderer closeSpriteRenderer = closeButton.GetComponent<SpriteRenderer>();
        closeSpriteRenderer.gameObject.transform.SetParent(SSPOptionsMenu.transform);
        closeSpriteRenderer.gameObject.transform.localPosition = new(2.25f, 2.44f, SSPOptionsMenu.transform.localPosition.z);
        closeSpriteRenderer.gameObject.layer = 5;
        closeSpriteRenderer.sortingOrder = 1;

        closeSpriteRenderer.sprite = Helpers.loadSpriteFromResources("SuperSimplePlus.Resources.CloseButton.png", 115f);

        closeSpriteRenderer.gameObject.SetActive(true);
        closeSpriteRenderer.enabled = true;

        closeButton.OnClick = new ButtonClickedEvent();

        closeButton.OnClick.AddListener((UnityAction)(() =>
        {
            GameObject.Destroy(SSPOptionsMenu);
        }));

        var title = Object.Instantiate(titleText, SSPOptionsMenu.transform);
        title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.0f;
        title.gameObject.layer = 5;
        title.gameObject.SetActive(true);
        title.text = ModTranslation.GetString("SSPSettings");
        title.name = "TitleText";

        SetUpOptions();
    }

    private static void SetUpOptions()
    {
        for (var i = 0; i < AllOptions.Length; i++)
        {
            var info = AllOptions[i];

            var button = Object.Instantiate(buttonPrefab, SSPOptionsMenu.transform);

            var pos = new Vector3(i % 2 == 0 ? -1.17f : 1.17f, 1.3f - i / 2 * 0.8f, -.5f);

            var transform = button.transform;
            transform.localPosition = pos;

            button.onState = info.DefaultValue;
            button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;

            button.Text.text = ModTranslation.GetString(info.Title);
            button.Text.fontSizeMin = button.Text.fontSizeMax = 2.5f;
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
        }
    }

    private static IEnumerable<GameObject> GetAllChilds(this GameObject Go)
    {
        for (var i = 0; i < Go.transform.childCount; i++)
        {
            yield return Go.transform.GetChild(i).gameObject;
        }
    }

    public class SelectionBehaviour
    {
        public string Title;
        public Func<bool> OnClick;
        public bool DefaultValue;

        public SelectionBehaviour(string title, Func<bool> onClick, bool defaultValue)
        {
            Title = title;
            OnClick = onClick;
            DefaultValue = defaultValue;
        }
    }
}
