using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Roles
{
    public class Miner : RoleWithCooldown
    {
        public readonly List<Vent> Vents = new List<Vent>();

        public KillButtonManager _mineButton;

        public Miner(PlayerControl player) : base(player, RoleEnum.Miner, CustomGameOptions.MineCd)
        {
            ImpostorText = () => "From the top, make it drop, that's a vent";
            TaskText = () => "From the top, make it drop, that's a vent";
        }

        public bool CanPlace { get; set; }
        public Vector2 VentSize { get; private set; }

        protected override void DoOnGameStart()
        {
            base.DoOnGameStart();
            var vents = Object.FindObjectsOfType<Vent>();
            VentSize =
                Vector2.Scale(vents[0].GetComponent<BoxCollider2D>().size, vents[0].transform.localScale) * 0.75f;
        }

        public KillButtonManager MineButton
        {
            get => _mineButton;
            set
            {
                _mineButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }
    }
}
