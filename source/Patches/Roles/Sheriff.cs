using System;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Sheriff : RoleWithCooldown
    {
        public Sheriff(PlayerControl player) : base(player, RoleEnum.Sheriff, CustomGameOptions.SheriffKillCd)
        {
            ImpostorText = () => "Shoot the <color=#FF0000FF>Impostor</color>";
            TaskText = () => "Kill off the impostor but don't kill crewmates.";
        }

        public PlayerControl ClosestPlayer;

        internal override bool Criteria()
        {
            return CustomGameOptions.ShowSheriff || base.Criteria();
        }
    }
}
