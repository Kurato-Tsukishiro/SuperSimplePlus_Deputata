using System;
using System.Runtime.CompilerServices;

namespace SuperSimplePlus
{
    public class Logger
    {
        public static void Info(string text, string tag = "", [CallerMemberName] string callerMethod = "") => SSPPlugin.Logger.LogInfo($"[{tag}:Info][{DateTime.Now.ToString()}][{callerMethod}]{text}");
        public static void Warning(string text, string tag = "", [CallerMemberName] string callerMethod = "") => SSPPlugin.Logger.LogWarning($"[{tag}:Warning][{DateTime.Now.ToString()}][{callerMethod}]{text}");
        public static void Error(string text, string tag = "", [CallerMemberName] string callerMethod = "") => SSPPlugin.Logger.LogError($"[{tag}:Error][{DateTime.Now.ToString()}][{callerMethod}]{text}");
        public static void Fatel(string text, string tag = "", [CallerMemberName] string callerMethod = "") => SSPPlugin.Logger.LogFatal($"[{tag}:Fatel][{DateTime.Now.ToString()}][{callerMethod}]{text}");
        public static void Message(string text, string tag = "", [CallerMemberName] string callerMethod = "") => SSPPlugin.Logger.LogMessage($"[{tag}][{DateTime.Now.ToString()}][{callerMethod}]{text}");
    }
}
