using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Reactor;
using Reactor.Extensions;
using Rewired;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.ProphetMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public class Revelation
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Prophet)) return;

            CheckForRevelation(__instance);
        }

        private static bool IsTimeForRevelation(PlayerControl __instance)
        {
            var now = DateTime.UtcNow;

            var role = Role.GetRole<Prophet>(PlayerControl.LocalPlayer);
            var timeSpan = now - role.LastRevealed;
            var cooldown = CustomGameOptions.ProphetCooldown * 1000f;

            return cooldown <= timeSpan.TotalMilliseconds;
        }

        private static void CheckForRevelation(PlayerControl __instance)
        {
            var role = Role.GetRole<Prophet>(PlayerControl.LocalPlayer);
            role.Revelation();
            Coroutines.Start(Utils.FlashCoroutine(role.Color));
        }
    }
}