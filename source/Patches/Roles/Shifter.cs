using System;
using TownOfUs.Extensions;
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

        public void Loses()
        {
            LostByRPC = true;
        }
    }
}
