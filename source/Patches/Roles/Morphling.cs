using System;
using TownOfUs.Extensions;
using TownOfUs.Roles.Modifiers;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Morphling : RoleWithCooldown, IVisualAlteration
    {
        public KillButtonManager _morphButton;
        public PlayerControl ClosestPlayer;
        public PlayerControl MorphedPlayer;

        public PlayerControl SampledPlayer;
        public float TimeRemaining;

        public Morphling(PlayerControl player) : base(player, RoleEnum.Morphling, CustomGameOptions.MorphlingCd)
        {
            ImpostorText = () => "Transform into crewmates";
            TaskText = () => "Morph into crewmates to be disguised";
        }

        public KillButtonManager MorphButton
        {
            get => _morphButton;
            set
            {
                _morphButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        protected override void DoOnMeetingEnd()
        {
            base.DoOnMeetingEnd();
            MorphButton.renderer.sprite = TownOfUs.SampleSprite;
            SampledPlayer = null;
        }

        public bool Morphed => TimeRemaining > 0f;

        public void Morph()
        {
            TimeRemaining -= Time.deltaTime;
            Utils.Morph(Player, MorphedPlayer);
        }

        public void Unmorph()
        {
            MorphedPlayer = null;
            Utils.Unmorph(Player);
            ResetCooldownTimer();
        }

        public bool TryGetModifiedAppearance(out VisualAppearance appearance)
        {
            if (Morphed)
            {
                appearance = MorphedPlayer.GetDefaultAppearance();
                var modifier = Modifier.GetModifier(MorphedPlayer);
                if (modifier is IVisualAlteration alteration)
                    alteration.TryGetModifiedAppearance(out appearance);
                return true;
            }

            appearance = Player.GetDefaultAppearance();
            return false;
        }
    }
}
