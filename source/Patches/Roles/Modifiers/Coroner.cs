using System.Collections.Generic;
using DepotDownloader;
using HarmonyLib;
using Hazel;
using Rewired;
using TownOfUs.ImpostorRoles.CamouflageMod;
using UnityEngine;

namespace TownOfUs.Roles.Modifiers
{
    public class Coroner : Modifier
    {
        private readonly HashSet<byte> _revealed = new HashSet<byte>();

        public Coroner(PlayerControl player) : base(player, ModifierEnum.Anthropomancer)
        {
            Name = "Coroner";
            TaskText = () => "Read the entrails of dead players to discover their role.";
            Color = new Color(0.20f, 0.40f, 0.16f);
        }

        public void Reveal(byte playerId)
        {
            _revealed.Add(playerId);
        }

        public bool HasRevealed(byte playerId)
        {
            return _revealed.Contains(playerId);
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

            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.AnthropomancerEat, SendOption.Reliable, -1);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(info.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            ((Coroner) modifier).Reveal(info.PlayerId);
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class ShowAnthropomancerPlayers
    {
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(HudManager __instance)
        {
            if (
                MeetingHud.Instance == null
                || PlayerControl.AllPlayerControls.Count <= 1
                || PlayerControl.LocalPlayer == null
                || PlayerControl.LocalPlayer.Data == null
                || Modifier.GetModifier(PlayerControl.LocalPlayer)?.ModifierType != ModifierEnum.Anthropomancer
                || (PlayerControl.LocalPlayer.Data.IsDead && CustomGameOptions.DeadSeeRoles)
            )
            {
                return;
            }

            UpdateMeeting(MeetingHud.Instance);
        }

        private static void UpdateMeeting(MeetingHud __instance)
        {
            Coroner coroner = Modifier.GetModifier<Coroner>(PlayerControl.LocalPlayer);
            foreach (PlayerVoteArea voteArea in __instance.playerStates)
            {
                if (!coroner.HasRevealed(voteArea.TargetPlayerId))
                {
                    continue;
                }

                PlayerControl player = Utils.PlayerById(voteArea.TargetPlayerId);
                Role role = Role.GetRole(player);
                voteArea.NameText.color = role.Color;

                if (CamouflageUnCamouflage.IsCamoed && CustomGameOptions.MeetingColourblind)
                {
                    // TODO: Do we need this?
                    // voteArea.NameText.text = player.name;
                }
                else
                {
                    voteArea.NameText.text = player.name + $" ({role.Name})";
                }
            }
        }
    }
}
