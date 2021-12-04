using UnityEngine;

namespace TownOfUs.Roles.Modifiers
{
    public class Diseased : Modifier
    {
        public Diseased(PlayerControl player) : base(player, ModifierEnum.Diseased)
        {
            Name = "Diseased";
            TaskText = () => "Killing you gives Impostors a high cooldown";
            Color = Color.grey;
        }
    }
}