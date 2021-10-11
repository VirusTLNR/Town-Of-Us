using System;
using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.ProphetMod
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public class Start
    {
        public static void Postfix(ShipStatus __instance)
        {
            foreach (var role in Role.GetRoles(RoleEnum.Prophet))
            {
                var prophet = (Prophet) role;
                prophet.LastRevealed = DateTime.UtcNow;

                if (CustomGameOptions.ProphetInitialReveal)
                {
                    prophet.Revelation();
                }
            }
        }
    }
}