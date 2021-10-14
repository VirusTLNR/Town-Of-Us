using System;
using System.Collections.Generic;
using System.Linq;
using Reactor;
using Reactor.Extensions;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Prophet : Role
    {
        public readonly ISet<byte> Revealed = new HashSet<byte>();

        public DateTime LastRevealed { get; set; }

        public Prophet(PlayerControl player) : base(player)
        {
            Name = "Prophet";
            ImpostorText = () => "Survive and find crewmates";
            TaskText = () => "Survive to find all the crewmates";
            Color = new Color(0.69f, 0.15f, 1f, 1f);
            RoleType = RoleEnum.Prophet;
            LastRevealed = DateTime.UtcNow;

            // I think this will trigger a revelation as soon as the HUD hits
            if (CustomGameOptions.ProphetInitialReveal)
            {
                LastRevealed = LastRevealed.AddMilliseconds(CustomGameOptions.ProphetCooldown * -1);
            }
        }

        public void Revelation()
        {
            LastRevealed = DateTime.UtcNow;

            List<PlayerControl> allPlayers = PlayerControl.AllPlayerControls.ToArray().ToList();

            PlayerControl target = allPlayers
                .Where(player => !Revealed.Contains(player.PlayerId))
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
            Revealed.Add(target.PlayerId);
        }
    }
}