using UnityEngine;

namespace TownOfUs.Roles.Modifiers
{
    public class Tiebreaker : Modifier
    {
        public Tiebreaker(PlayerControl player) : base(player, ModifierEnum.Tiebreaker)
        {
            Name = "Tiebreaker";
            TaskText = () => "Your vote breaks ties";
            Color = new Color(0.6f, 0.9f, 0.6f);
        }
    }
}