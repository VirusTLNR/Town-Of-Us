using System.Reflection;
using HarmonyLib;

namespace TownOfUs
{
    [HarmonyPriority(Priority.VeryHigh)] // to show this message first, or be overrided if any plugins do
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionShowerUpdate
    {
        public static string versionShowerString = "<color=#00FF00FF>TownOfUs " + TownOfUs.GetVersion() + "</color>";

        public static void Postfix(VersionShower __instance)
        {
            var text = __instance.text;
            text.text += " - "+ versionShowerString;
        }
    }
}
