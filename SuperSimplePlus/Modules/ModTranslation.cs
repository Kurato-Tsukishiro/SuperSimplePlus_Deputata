//参考 => https://github.com/ykundesu/SuperNewRoles/blob/1.8.1.2/SuperNewRoles/Modules/ModTranslation.cs
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AmongUs.Data;

namespace SuperSimplePlus;
public static class ModTranslation
{
    // 一番左と一行全部
    private static Dictionary<string, string[]> dictionary = new();
    internal static string GetString(string key)
    {
        // アモアス側の言語読み込みが完了しているか ? 今の言語 : 最後の言語
        SupportedLangs langId = TranslationController.InstanceExists ? TranslationController.Instance.currentLanguage.languageID : DataManager.Settings.Language.CurrentLanguage;

        if (!dictionary.ContainsKey(key)) return key; // keyが辞書にないならkeyのまま返す

        return langId switch
        {
            SupportedLangs.English => dictionary[key][1], // 英語
            SupportedLangs.Japanese => dictionary[key][2],// 日本語
            _ => dictionary[key][1] // それ以外は英語
        };
    }

    internal static void LoadCsv()
    {
        var fileName = Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperSimplePlus.Resources.Translate.csv");

        //csvを開く
        StreamReader sr = new(fileName);

        var i = 0;
        //1行ずつ処理
        while (!sr.EndOfStream)
        {
            try
            {
                // 行ごとの文字列
                string line = sr.ReadLine();

                // 行が空白 戦闘が*なら次の行に
                if (line == "" || line[0] == '#') continue;

                //カンマで配列の要素として分ける
                string[] values = line.Split(',');

                // 配列から辞書に格納する
                List<string> valuesList = new();
                foreach (string vl in values)
                {
                    valuesList.Add(vl.Replace("\\n", "\n").Replace("，", ","));
                }
                dictionary.Add(values[0], valuesList.ToArray());
                i++;
            }
            catch
            {
                Logger.Error($"Error: Loading Translate.csv Line:{i}", "ModTranslation");
            }
        }
    }
}
