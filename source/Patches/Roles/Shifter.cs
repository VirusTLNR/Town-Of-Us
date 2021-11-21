using System;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Shifter : RoleWithCooldown
    {
        public Shifter(PlayerControl player) : base(player, RoleEnum.Shifter, CustomGameOptions.ShifterCd)
        {
            ImpostorText = () => "Shift around different roles";
            TaskText = () => "Steal other people's roles.\nFake Tasks:";
        }

        public PlayerControl ClosestPlayer;
        public DateTime LastShifted { get; set; }

        public void Loses()
        {
            Player.Data.IsImpostor = true;
        }
    }
}
