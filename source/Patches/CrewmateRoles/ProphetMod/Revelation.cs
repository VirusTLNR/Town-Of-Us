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
            role.LastRevealed = DateTime.UtcNow;

            var allPlayers = PlayerControl.AllPlayerControls.ToArray().ToList();

            var target = allPlayers
                .Where(player => !role.Revealed.Contains(player.PlayerId))
                .Where(player => Role.GetRole(player).Faction == Faction.Crewmates)
                .Random();

            if (target == null)
            {
                PluginSingleton<TownOfUs>.Instance.Log.LogMessage(
                    $"The Prophet has no more eligible revelations to receive.");
                return;
            }

            PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"The Prophet has received information that {target.nameText} is a Crewmate role. "
                                                              + $"Their role is {Role.GetRole(target).Name}. They are currently {(target.Data.IsDead ? "dead" : "alive")}.");
            Coroutines.Start(Utils.FlashCoroutine(role.Color));
            role.Revealed.Add(target.PlayerId);
        }
    }
}