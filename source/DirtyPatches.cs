using System;
using System.Reflection;
using HarmonyLib;
using Hazel.Udp;
using Reactor;

namespace TownOfUs
{
    public static class DirtyPatches
    {
        public static void Initialize(Harmony harmony)
        {
            Logger<TownOfUs>.Instance.LogDebug($"[{DateTime.Now.ToString("yyyy-MM-dd@hh:mm:ss")}]: Start Of [DirtyPatches|{ MethodBase.GetCurrentMethod().Name}]");
            try
            {
                harmony.Unpatch(
                    AccessTools.Method(typeof(UdpConnection), nameof(UdpConnection.HandleSend)),
                    HarmonyPatchType.Prefix,
                    ReactorPlugin.Id
                );
            }
            catch (Exception e)
            {
                Logger<TownOfUs>.Instance.LogError($"Exception unpatching Reactor's UdpConnection.HandleSend Prefix: {e.Message}, Stack: {e.StackTrace}");
            }
            Logger<TownOfUs>.Instance.LogDebug($"[{DateTime.Now.ToString("yyyy-MM-dd@hh:mm:ss")}]: End Of [DirtyPatches|{ MethodBase.GetCurrentMethod().Name}]");
        }
    }
}