using System.Collections.Generic;
using System.IO;
using System.Reflection;
using InnerNet;
using AmongUs.Data;

namespace SuperSimplePlus;
public static class ImmigrationCheck
{
    // 一番左と一行全部
    private static Dictionary<uint, string> dictionary = new(); //keyを行番号, valueをフレンドコードに
    internal static void DenyEntryToFriendCode(ClientData client, string entryFriendCode)
    {
        if (!SSPPlugin.FriendCodeBan.Value) return;
        if (dictionary.ContainsValue(entryFriendCode))
            AmongUsClient.Instance.KickPlayer(client.Id, ban: true);
    }

    internal static void LoadFriendCodeList()
    {
    }
}
