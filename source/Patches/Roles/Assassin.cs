using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Assassin : IMeetingGuesser
    {
        public static Assassin AssassinState { get; set; }
        public Dictionary<byte, (GameObject, GameObject, TMP_Text)> Buttons { get; } = new Dictionary<byte, (GameObject, GameObject, TMP_Text)>();
        public Dictionary<byte, int> Guesses { get; } = new Dictionary<byte, int>();

        public List<RoleEnum> PossibleGuesses { get; }

        public Assassin()
        {
            RemainingKills = CustomGameOptions.AssassinKills;

            PossibleGuesses = CustomGameOptions.AssassinGuessNeutrals
                ? CustomGameOptions.GetEnabledRoles(Faction.Crewmates, Faction.Neutral)
                : CustomGameOptions.GetEnabledRoles(Faction.Crewmates);

            if (CustomGameOptions.AssassinCrewmateGuess)
                PossibleGuesses.Add(RoleEnum.Crewmate);
        }

        public int RemainingKills { get; set; }

        public bool CanKeepGuessing() => RemainingKills > 0
                                         && !CustomGameOptions.AssassinMultiKill;
    }
}
