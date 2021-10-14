using UnityEngine;

namespace TownOfUs.Roles
{
    public class Engineer : Role
    {
        public Engineer(PlayerControl player) : base(player)
        {
            Name = "Engineer";
            ImpostorText = () => "Maintain important systems on the ship";
            TaskText = () => "Vent and fix a sabotage from anywhere!";
            Color = new Color(1f, 0.65f, 0.04f, 1f);
            RoleType = RoleEnum.Engineer;
        }

        protected override void DoOnMeetingEnd()
        {
            if (CustomGameOptions.EngineerFixPer == EngineerFixPer.Round)
            {
                UsedThisRound = false;
            }
        }
        public bool UsedThisRound { get; set; } = false;

        public enum EngineerFixPer
        {
            Round,
            Game
        }
    }
}