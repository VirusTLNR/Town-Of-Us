using System;

namespace TownOfUs.Roles
{
    public abstract class RoleWithCooldown : Role
    {
        public DateTime LastUsedAbility;
        private readonly float _cooldown;
        protected RoleWithCooldown(PlayerControl player, RoleEnum roleEnum, float cooldown) : base(player, roleEnum)
        {
            this._cooldown = cooldown;
        }

        protected override void DoOnGameStart()
        {
            float discount = PlayerControl.GameOptions.KillCooldown - CustomGameOptions.InitialImpostorKillCooldown;
            LastUsedAbility = DateTime.UtcNow.AddSeconds(-1 * discount);
        }

        protected override void DoOnMeetingEnd()
        {
            LastUsedAbility = DateTime.UtcNow;
        }

        public float CooldownTimer()
        {
            return Utils.GetCooldownTimeRemaining(() => LastUsedAbility, () => _cooldown);
        }
    }
}