using System;
using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.Patches.CrewmateRoles.ProphetMod
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public class PostMeeting
    {
        public static void Postfix()
        {
            foreach (var role in Role.GetRoles(RoleEnum.Prophet))
            {
                var prophet = (Prophet) role;
                prophet.LastRevealed = DateTime.UtcNow;
            }
        }
    }
}