using System;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Camouflager : RoleWithCooldown

    {
        private KillButton _camouflageButton;
        public bool Enabled;
        public float TimeRemaining;

        public Camouflager(PlayerControl player) : base(player, RoleEnum.Camouflager, CustomGameOptions.CamouflagerDuration)
        {
            ImpostorText = () => "Camouflage and turn everyone grey";
            TaskText = () => "Camouflage and get secret kills";
        }

        public bool Camouflaged => TimeRemaining > 0f;

        public KillButton CamouflageButton
        {
            get => _camouflageButton;
            set
            {
                _camouflageButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        public void Camouflage()
        {
            Enabled = true;
            TimeRemaining -= Time.deltaTime;
            Utils.Camouflage();
        }

        public void UnCamouflage()
        {
            Enabled = false;
            ResetCooldownTimer();
            Utils.UnCamouflage();
        }
    }
}
