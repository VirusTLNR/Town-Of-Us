using System;
using System.Collections.Generic;
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
            Color = new Color(0.19f, 0.09f, 0.20f, 1f);
            RoleType = RoleEnum.Prophet;
        }
    }
}