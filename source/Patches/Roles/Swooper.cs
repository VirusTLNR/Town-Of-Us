using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Roles
{
    public class Swooper : Role
    {
        public KillButtonManager _swoopButton;
        public bool Enabled;
        public DateTime LastSwooped;
        public float TimeRemaining;

        public Swooper(PlayerControl player) : base(player)
        {
            Name = "Swooper";
            ImpostorText = () => "Turn invisible temporarily";
            TaskText = () => "Turn invisible and sneakily kill";
            Color = Palette.ImpostorRed;
            RoleType = RoleEnum.Swooper;
            Faction = Faction.Impostors;
        }

        protected override void DoOnGameStart()
        {
            LastSwooped = DateTime.UtcNow;
        }

        protected override void DoOnMeetingEnd()
        {
            LastSwooped = DateTime.UtcNow;
        }
        public bool IsSwooped => TimeRemaining > 0f;

        public KillButtonManager SwoopButton
        {
            get => _swoopButton;
            set
            {
                _swoopButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        public float SwoopTimer()
        {
            return Utils.GetCooldownTimeRemaining(() => LastSwooped, () => CustomGameOptions.SwoopCd);
        }

        public void Swoop()
        {
            Enabled = true;
            TimeRemaining -= Time.deltaTime;
            Utils.MakeInvisible(Player, PlayerControl.LocalPlayer.Data.IsImpostor || PlayerControl.LocalPlayer.Data.IsDead);
        }

        public void UnSwoop()
        {
            Enabled = false;
            LastSwooped = DateTime.UtcNow;
            Utils.MakeVisible(Player);
        }
    }
}
