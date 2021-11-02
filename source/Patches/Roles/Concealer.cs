using System;
using TownOfUs.ImpostorRoles.CamouflageMod;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Concealer : Role
    {
        private KillButtonManager _concealButton;
        private DateTime _lastConcealed;
        public float TimeBeforeConcealed;
        public float ConcealTimeRemaining;
        public PlayerControl Target;
        public PlayerControl Concealed;

        public Concealer(PlayerControl player) : base(player)
        {
            Name = "Concealer";
            ImpostorText = () => "Conceal crewmates from each other for a sneaky kill";
            TaskText = () => "Conceal crewmates from each other for a sneaky kill";
            Color = Palette.ImpostorRed;
            RoleType = RoleEnum.Concealer;
            Faction = Faction.Impostors;
        }

        protected override void DoOnGameStart()
        {
            _lastConcealed = DateTime.UtcNow;
            Target = null;
        }

        protected override void DoOnMeetingEnd()
        {
            _lastConcealed = DateTime.UtcNow;
            Target = null;
        }

        public KillButtonManager ConcealButton
        {
            get => _concealButton;
            set
            {
                _concealButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        public float ConcealTimer()
        {
            return Utils.GetCooldownTimeRemaining(() => _lastConcealed, () => CustomGameOptions.ConcealCooldown);
        }

        public void StartConceal(PlayerControl concealed)
        {
            Concealed = concealed;
            TimeBeforeConcealed = CustomGameOptions.TimeToConceal;
        }

        /*
         * Called every time incremement. Will manage the time remaining and transition from Waiting to Conceal
         * to Concealed to Unconcealed.
         */
        public void ConcealTick()
        {
            if (Concealed == null)
            {
                return;
            }

            if (TimeBeforeConcealed > 0)
            {
                TimeBeforeConcealed -= Time.deltaTime;
                if (TimeBeforeConcealed <= 0f)
                {
                    ConcealTimeRemaining = CustomGameOptions.ConcealDuration;
                }
            }
            else if (ConcealTimeRemaining > 0)
            {
                ConcealTimeRemaining -= Time.deltaTime;
                Conceal();
            }
            else
            {
                Unconceal();
            }
        }

        private void Conceal()
        {
            // If the local player is an impostor, we don't actually want to swoop them
            if (
                PlayerControl.LocalPlayer.Data.IsImpostor
                || PlayerControl.LocalPlayer.Data.IsDead
                || CamouflageUnCamouflage.IsCamoed
                || Concealed == null
                || PlayerControl.LocalPlayer.PlayerId == Concealed.PlayerId
                )
            {
                return;
            }

            Utils.MakeInvisible(Concealed, false);
        }

        private void Unconceal()
        {
            _lastConcealed = DateTime.UtcNow;
            if (Concealed != null)
            {
                Utils.Unmorph(Concealed);
                Concealed = null;
            }
        }
    }
}
