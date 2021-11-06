using System;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Covert : Role
    {
        private KillButtonManager __covertButton;
        public DateTime LastCovert;
        public float CovertTimeRemaining;

        public Covert(PlayerControl player) : base(player)
        {
            Name = "Covert";
            ImpostorText = () => "Do your tasks. Covertly.";
            TaskText = () => "Do your tasks. Covertly.";
            Color = new Color(0.48f, 0.50f, 0.10f, 1f);
            RoleType = RoleEnum.Covert;
            Faction = Faction.Crewmates;
        }

        protected override void DoOnGameStart()
        {
            LastCovert = DateTime.UtcNow;
            CovertTimeRemaining = 0f;
        }

        protected override void DoOnMeetingEnd()
        {
            LastCovert = DateTime.UtcNow;
            CovertTimeRemaining = 0f;
        }

        public KillButtonManager CovertButton
        {
            get => __covertButton;
            set
            {
                __covertButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        public bool IsCovert => CovertTimeRemaining > 0f;

        public float CovertTimer()
        {
            return Utils.GetCooldownTimeRemaining(() => LastCovert, () => CustomGameOptions.CovertCooldown);
        }

        public void CovertTick()
        {
            if (IsCovert)
            {
                CovertTimeRemaining -= Time.deltaTime;
            }
            else
            {
                LeaveCovert();
            }
        }

        public void GoCovert()
        {
            CovertTimeRemaining = CustomGameOptions.CovertDuration;
            Utils.MakeInvisible(Player, PlayerControl.LocalPlayer.Data.IsDead);
        }

        public void LeaveCovert()
        {
            LastCovert = DateTime.UtcNow;
            Utils.MakeVisible(Player);
        }
    }
}