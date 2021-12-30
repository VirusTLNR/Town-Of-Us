using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Roles
{
    public class Swooper : RoleWithCooldown
    {
        public KillButton _swoopButton;
        public bool Enabled;
        public float TimeRemaining;

        public Swooper(PlayerControl player) : base(player, RoleEnum.Swooper, CustomGameOptions.SwoopCd)
        {
            ImpostorText = () => "Turn invisible temporarily";
            TaskText = () => "Turn invisible and sneakily kill";
        }

        public bool IsSwooped => TimeRemaining > 0f;

        public KillButton SwoopButton
        {
            get => _swoopButton;
            set
            {
                _swoopButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        public void Swoop()
        {
            Enabled = true;
            TimeRemaining -= Time.deltaTime;
            Utils.MakeInvisible(Player, PlayerControl.LocalPlayer.Data.IsImpostor() || PlayerControl.LocalPlayer.Data.IsDead);
        }

        public void UnSwoop()
        {
            Enabled = false;
            ResetCooldownTimer();
            Utils.MakeVisible(Player);
        }
    }
}
