using UnityEngine;

namespace TownOfUs.Roles.Modifiers
{
    public class Torch : Modifier
    {
        public Torch(PlayerControl player) : base(player, ModifierEnum.Torch)
        {
            Name = "Torch";
            TaskText = () => "You can see in the dark.";
            Color = new Color(1f, 1f, 0.6f);
        }
    }
}