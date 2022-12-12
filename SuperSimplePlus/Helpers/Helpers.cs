using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Hazel;
using SuperSimplePlus.Modules;
using SuperSimplePlus.Patches;
using UnhollowerBaseLib;
using UnityEngine;

namespace SuperSimplePlus
{
    public class Helpers
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
    }
}
