using System;
using System.Collections.Generic;
using TownOfUs.CrewmateRoles.SeerMod;
using TownOfUs.Extensions;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Seer : RoleWithCooldown
    {
        public readonly Dictionary<byte, bool> Investigated = new Dictionary<byte, bool>();

        public Seer(PlayerControl player) : base(player, RoleEnum.Seer, CustomGameOptions.SeerCd)
        {
            ImpostorText = () => "Investigate roles";
            TaskText = () => "Investigate roles and find the Impostor";
        }

        public PlayerControl ClosestPlayer;

        public bool CheckSeeReveal(PlayerControl player)
        {
            var role = GetRole(player);
            switch (CustomGameOptions.SeeReveal)
            {
                case SeeReveal.All:
                    return true;
                case SeeReveal.Nobody:
                    return false;
                case SeeReveal.ImpsAndNeut:
                    return role != null && role.Faction != Faction.Crewmates || player.Data.IsImpostor();
                case SeeReveal.Crew:
                    return role != null && role.Faction == Faction.Crewmates || !player.Data.IsImpostor();
            }

            return false;
        }
    }
}
