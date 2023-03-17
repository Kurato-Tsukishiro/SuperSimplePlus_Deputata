using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperSimplePlus.Modules;
using SuperSimplePlus.Patches;
using UnhollowerBaseLib;
using UnityEngine;

namespace SuperSimplePlus;
public static class Helpers
{
    public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit)
    {
        try
        {
            Texture2D texture = loadTextureFromResources(path);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
        catch
        {
            System.Console.WriteLine("Error loading sprite from path: " + path);
        }
        return null;
    }

    public static unsafe Texture2D loadTextureFromResources(string path)
    {
        try
        {
            Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            var length = stream.Length;
            var byteTexture = new Il2CppStructArray<byte>(length);
            stream.Read(new Span<byte>(IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(), (int)length));
            ImageConversion.LoadImage(texture, byteTexture, false);
            return texture;
        }
        catch
        {
            System.Console.WriteLine("Error loading texture from resources: " + path);
        }
        return null;
    }
    /// <summary>
    /// keyCodesが押されているか
    ///参考=>https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Patches/ShareGameVersionPatch.cs
    /// </summary>
    public static bool GetManyKeyDown(KeyCode[] keyCodes) =>
        keyCodes.All(x => Input.GetKey(x)) && keyCodes.Any(x => Input.GetKeyDown(x));

    /// <summary>
    /// PlayerControl型のオブジェクトをClientData型に変換する。(GetPlayer()の逆) 参考=>https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/ModHelpers.cs
    /// </summary>
    /// <param name="player">変換したいPlayerControl型のオブジェクト</param>
    /// <returns>cd : 変換されたClientData型のオブジェクト</returns>
    public static ClientData GetClient(this PlayerControl player) =>
        AmongUsClient.Instance.allClients.ToArray().Where(cd => cd.Character.PlayerId == player.PlayerId).FirstOrDefault();

    /// <summary>
    ///　ClientData型のオブジェクトをPlayerControl型に変換する。(GetClient()の逆)
    /// </summary>
    /// <param name="client">変換したいClientData型のオブジェクト</param>
    /// <returns>pl : 変換されたPlayerControl型のオブジェクト</returns>
    public static PlayerControl GetPlayer(this ClientData client) =>
        PlayerControl.AllPlayerControls.ToArray().Where(pl => pl.PlayerId == client.Character.PlayerId).FirstOrDefault();

    /*
        参考=>https://github.com/tugaru1975/TownOfPlus/blob/main/Helpers.cs
        GetColorName(ClientData client), TryGetPlayerColor(int colorId, out string color), GetTranslation(this StringNames name)
    */
    public static string GetColorName(ClientData client) => TryGetPlayerColor(client.ColorId, out var color) ? color : "";

    public static bool TryGetPlayerColor(int colorId, out string color)
    {
        color = "";
        if (Palette.ColorNames.Length <= colorId) return false;

        color = Palette.ColorNames[colorId].GetTranslation();
        return true;
    }

    public static string GetTranslation(this StringNames name) =>
        DestroyableSingleton<TranslationController>.Instance.GetString(name, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
}
