using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.ImpostorRoles.AssassinMod
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public class AddButton
    {
        private static bool IsExempt(PlayerControl player) {
            if (
                player.Data.IsImpostor
            )
            {
                return true;
            }

            var role = Role.GetRole(player);
            return role != null && role.Criteria();
        }

        public static void Postfix(MeetingHud __instance)
        {
            Assassin.AssassinState.Guesses.Clear();
            Assassin.AssassinState.Buttons.Clear();

            if (
                PlayerControl.LocalPlayer.Data.IsDead
                || !PlayerControl.LocalPlayer.Is(Faction.Impostors)
            )
            {
                return;
            }

            Assassin assassin = Assassin.AssassinState;
            if (assassin.RemainingKills <= 0) return;
            foreach (var voteArea in __instance.playerStates)
            {
                IMeetingGuesser.GenButton(
                    assassin,
                    voteArea,
                    playerControl => !IsExempt(playerControl),
                    (playerControl, role) =>
                    {
                        IMeetingGuesser.KillFromMeetingGuess(assassin, playerControl, role);
                        assassin.RemainingKills--;
                    });
            }
        }

        public static void MaybeHideButtons()
        {
            Assassin assassin = Assassin.AssassinState;
            if (!assassin.CanKeepGuessing())
            {
                ShowHideButtons.HideButtons(assassin);
            }
        }
    }
}
