using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
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
        public int RemainingKills { get; set; }

        public Assassin()
        {
            RemainingKills = CustomGameOptions.AssassinKills;

            PossibleGuesses = CustomGameOptions.AssassinGuessNeutrals
                ? CustomGameOptions.GetEnabledRoles(Faction.Crewmates, Faction.Neutral)
                : CustomGameOptions.GetEnabledRoles(Faction.Crewmates);

            if (CustomGameOptions.AssassinCrewmateGuess)
                PossibleGuesses.Add(RoleEnum.Crewmate);
        }

        public bool CanKeepGuessing() => RemainingKills > 0
                                         && !CustomGameOptions.AssassinMultiKill;
    }

    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    public static class LobbyBehaviour_Start
    {
        private static void Postfix(LobbyBehaviour __instance)
        {
            Assassin.AssassinState = new Assassin();
        }
    }
}
