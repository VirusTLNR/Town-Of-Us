using System.Collections.Generic;
using DepotDownloader;
using HarmonyLib;
using Rewired;
using TownOfUs.ImpostorRoles.CamouflageMod;
using UnityEngine;

namespace TownOfUs.Roles.Modifiers
{
    // TODO: Make it work with shifting
    // TODO: Document in README

    public class Anthropomancer : Modifier
    {
        public readonly HashSet<byte> Eaten = new HashSet<byte>();

        public Anthropomancer(PlayerControl player) : base(player, ModifierEnum.Anthropomancer)
        {
            Name = "Anthropomancer";
            TaskText = () => "Read the entrails of dead players to discover their role.";
            Color = new Color(0.25f, 0.30f, 0.21f);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    public class BodyReportPatch
    {
        private static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo info)
        {
            Modifier modifier = Modifier.GetModifier(__instance);
            if (
                info == null
                || modifier == null
                || modifier.ModifierType != ModifierEnum.Anthropomancer
                )
            {
                return;
            }

            ((Anthropomancer) modifier).Eaten.Add(info.PlayerId);
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class ShowEatenPlayers
    {
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(HudManager __instance)
        {
            if (
                MeetingHud.Instance == null
                || PlayerControl.AllPlayerControls.Count <= 1
                || PlayerControl.LocalPlayer == null
                || PlayerControl.LocalPlayer.Data == null
                || Modifier.GetModifier(PlayerControl.LocalPlayer).ModifierType != ModifierEnum.Anthropomancer
                || (PlayerControl.LocalPlayer.Data.IsDead && CustomGameOptions.DeadSeeRoles)
            )
            {
                return;
            }

            UpdateMeeting(MeetingHud.Instance);
        }

        private static void UpdateMeeting(MeetingHud __instance)
        {
            Anthropomancer anthropomancer = Modifier.GetModifier<Anthropomancer>(PlayerControl.LocalPlayer);
            foreach (PlayerVoteArea voteArea in __instance.playerStates)
            {
                if (!anthropomancer.Eaten.Contains(voteArea.TargetPlayerId))
                {
                    return;
                }

                PlayerControl player = Utils.PlayerById(voteArea.TargetPlayerId);
                Role role = Role.GetRole(player);
                voteArea.NameText.color = role.Color;

                if (CamouflageUnCamouflage.IsCamoed && CustomGameOptions.MeetingColourblind)
                {
                    // TODO: Do we need this?
                    // player.nameText.text = player.name;
                }
                else
                {
                    player.nameText.text = player.name + $" ({role.Name})";
                }
            }
        }
    }
}
