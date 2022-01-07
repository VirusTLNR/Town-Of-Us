using System;
using TownOfUs.CrewmateRoles.TimeLordMod;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class TimeLord : RoleWithCooldown
    {
        public TimeLord(PlayerControl player) : base(player, RoleEnum.TimeLord, CustomGameOptions.RewindCooldown)
        {
            ImpostorText = () => "Rewind Time";
            TaskText = () => "Rewind Time!";
        }

        public float TimeRemaining;

        public bool IsRewinding => TimeRemaining > 0f;
    }
}
