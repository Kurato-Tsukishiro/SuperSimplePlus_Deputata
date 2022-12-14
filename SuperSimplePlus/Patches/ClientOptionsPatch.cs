//参考=>https://github.com/Eisbison/TheOtherRoles/blob/main/TheOtherRoles/Patches/ClientOptionsPatch.cs
using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace SuperSimplePlus.Patches
{
    [HarmonyPatch]
    public static class ClientOptionsPatch
    {
        private static readonly SelectionBehaviour[] AllOptions =
        {
            new(ModTranslation.getString("NotPCKick"),()=> SSPPlugin.NotPCKick.Value = !SSPPlugin.NotPCKick.Value,SSPPlugin.NotPCKick.Value),
            new(ModTranslation.getString("NotPCBan"),()=> SSPPlugin.NotPCBan.Value = !SSPPlugin.NotPCBan.Value,SSPPlugin.NotPCBan.Value),
        };

        public static GameObject SSPSettingButton;

        public static GameObject SSPOptionsMenu;
        private static TextMeshPro titleText;

        private static ToggleButtonBehaviour buttonPrefab;


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static void MainMenuManager_StartPostfix(MainMenuManager __instance)
        {
            // Prefab for the title
            var tmp = __instance.Announcement.transform.Find("Title_Text").gameObject.GetComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.transform.localPosition += Vector3.left * 0.2f;
            titleText = Object.Instantiate(tmp);
            Object.Destroy(titleText.GetComponent<TextTranslatorTMP>());
            titleText.gameObject.SetActive(false);
            Object.DontDestroyOnLoad(titleText);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
        public static void HudManager_StartPostfix(HudManager __instance)
        {
            SSPSettingButton = GameObject.Instantiate(HudManager.Instance.MapButton.gameObject);
            SSPSettingButton.transform.SetParent(HudManager.Instance.transform.FindChild("Buttons").FindChild("TopRight"));

            SSPSettingButton.SetActive(true);

            SSPSettingButton.GetComponent<SpriteRenderer>().sprite = Helpers.loadSpriteFromResources("SuperSimplePlus.Resources.SettingButton.png", 115f);

            ButtonBehavior SSPSettingButtonButtonBehavior = SSPSettingButton.GetComponent<ButtonBehavior>();
            SSPSettingButtonButtonBehavior.OnClick = new ButtonClickedEvent();
            SSPSettingButtonButtonBehavior.OnClick.AddListener((UnityAction)(() => { SSPSettingButtonOnClick(); }));

            buttonPrefab = Object.Instantiate(HudManager.Instance.transform.FindChild("Menu").GetComponent<OptionsMenuBehaviour>().CensorChatButton);
            Object.DontDestroyOnLoad(buttonPrefab);
            buttonPrefab.name = "CensorChatPrefab";
            buttonPrefab.gameObject.SetActive(false);

            SSPSettingButton.transform.localPosition = new(SSPSettingButton.transform.localPosition.x, SSPSettingButton.transform.localPosition.y - 0.75f, SSPSettingButton.transform.localPosition.z);
        }

        private static void SSPSettingButtonOnClick()
        {
            if (SSPOptionsMenu) return;

            SSPOptionsMenu = Object.Instantiate(HudManager.Instance.transform.FindChild("Menu").GetComponent<OptionsMenuBehaviour>().Background.gameObject);
            SSPOptionsMenu.transform.SetParent(HudManager.Instance.transform);
            SSPOptionsMenu.transform.position = HudManager.Instance.transform.FindChild("Menu").GetComponent<OptionsMenuBehaviour>().transform.position;
            SSPOptionsMenu.transform.localScale = new(SSPOptionsMenu.transform.localScale.x * 0.9f, SSPOptionsMenu.transform.localScale.y * 0.9f, SSPOptionsMenu.transform.localScale.z);

            GameObject SSPOptionsMenuCloseButton = GameObject.Instantiate(HudManager.Instance.MapButton.gameObject);
            SSPOptionsMenuCloseButton.SetActive(true);
            SSPOptionsMenuCloseButton.transform.SetParent(SSPOptionsMenu.transform);
            SSPOptionsMenuCloseButton.transform.localPosition = new(2.25f, 2.44f, SSPOptionsMenuCloseButton.transform.localPosition.z);

            SpriteRenderer OptionsMenuCloseButtonSpriteRender = SSPOptionsMenuCloseButton.GetComponent<SpriteRenderer>();
            OptionsMenuCloseButtonSpriteRender.sortingOrder = 1;
            OptionsMenuCloseButtonSpriteRender.sprite = Helpers.loadSpriteFromResources("SuperSimplePlus.Resources.CloseButton.png", 115f);

            ButtonBehavior OptionsMenuCloseButtonButtonBehaviour = SSPOptionsMenuCloseButton.GetComponent<ButtonBehavior>();
            OptionsMenuCloseButtonButtonBehaviour.OnClick = new ButtonClickedEvent();
            OptionsMenuCloseButtonButtonBehaviour.OnClick.AddListener((UnityAction)(() =>
            {
                GameObject.Destroy(SSPOptionsMenu);
                PlayerControl.LocalPlayer.moveable = true;
            }));

            var title = Object.Instantiate(titleText, SSPOptionsMenu.transform);
            title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
            title.gameObject.SetActive(true);
            title.text = ModTranslation.getString("SSPSettings");
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

                button.Text.text = ModTranslation.getString(info.Title);
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
}
