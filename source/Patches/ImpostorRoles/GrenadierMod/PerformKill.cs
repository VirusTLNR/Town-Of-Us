using HarmonyLib;
using Hazel;
using TownOfUs.Roles;
using System.Linq;

namespace TownOfUs.ImpostorRoles.GrenadierMod
{
    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    public class PerformKill
    {
        public static bool Prefix(KillButtonManager __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Grenadier))
            {
                return true;
            }
            if (!PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.Data.IsDead)
            {
                return false;
            }
            var role = Role.GetRole<Grenadier>(PlayerControl.LocalPlayer);
            if (__instance != role.FlashButton)
            {
                return true;
            }
            if (
                __instance.isCoolingDown
                || !__instance.isActiveAndEnabled
                || Utils.IsSabotageActive()
                || role.CooldownTimer() != 0
            )
            {
                return false;
            }

            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.FlashGrenade, SendOption.Reliable, -1);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            role.TimeRemaining = CustomGameOptions.GrenadeDuration;
            role.Flash();
            return false;
        }
    }
}
