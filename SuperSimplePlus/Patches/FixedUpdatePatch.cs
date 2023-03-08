using System.Collections.Generic;

namespace SuperSimplePlus.Patches;

public static class FixedUpdate
{
    public static Dictionary<int, string> DefaultName = new();
    // 参考 => https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Mode/SuperHostRoles/FixedUpdate.cs
    public static string GetDefaultName(this PlayerControl player)
    {
        var plId = player.PlayerId;

        if (DefaultName.ContainsKey(plId))
            return DefaultName[plId];
        else
        {
            DefaultName[plId] = player.Data.PlayerName;
            return DefaultName[plId];
        }
    }
}
