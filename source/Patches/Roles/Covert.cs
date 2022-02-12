using System;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Covert : RoleWithCooldown
    {
        private KillButton _covertButton;
        public float CovertTimeRemaining;
        public bool IsCovert { get; private set; }

        public Covert(PlayerControl player) : base(player, RoleEnum.Covert, CustomGameOptions.CovertCooldown)
        {
            ImpostorText = () => "Do your tasks. Covertly.";
            TaskText = () => "Do your tasks. Covertly.";
        }

        protected override void DoOnGameStart()
        {
            base.DoOnGameStart();
            CovertTimeRemaining = 0f;
        }

        protected override void DoOnMeetingEnd()
        {
            base.DoOnMeetingEnd();
            CovertTimeRemaining = 0f;
        }

        public KillButton CovertButton
        {
            get => _covertButton;
            set
            {
                _covertButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        public void CovertTick()
        {
            if (!IsCovert)
            {
                return;
            }

            if (CovertTimeRemaining > 0f)
            {
                // We have to apply invisibiility every tick, otherwise the default name color will make the name appear
                CovertTimeRemaining -= Time.deltaTime;
                Utils.MakeInvisible(Player, PlayerControl.LocalPlayer.Is(RoleEnum.Covert) || PlayerControl.LocalPlayer.Data.IsDead);
            }
            else
            {
                LeaveCovert();
            }
        }

        public void GoCovert()
        {
            IsCovert = true;
            CovertTimeRemaining = CustomGameOptions.CovertDuration;
            //Utils.MakeInvisible(Player, PlayerControl.LocalPlayer.Is(RoleEnum.Covert) || PlayerControl.LocalPlayer.Data.IsDead);
        }

        private void LeaveCovert()
        {
            IsCovert = false;
            ResetCooldownTimer();
            Utils.MakeVisible(Player);
        }
    }
}