using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Arsonist : RoleWithCooldown
    {
        private KillButtonManager _igniteButton;
        public bool ArsonistWins;
        public PlayerControl ClosestPlayer;
        public readonly List<byte> DousedPlayers = new List<byte>();
        public bool IgniteUsed;


        public Arsonist(PlayerControl player) : base(player, RoleEnum.Arsonist, CustomGameOptions.DouseCd)
        {
            ImpostorText = () => "Douse players and ignite the light";
            TaskText = () => "Douse players and ignite to kill everyone\nFake Tasks:";
        }

        public KillButtonManager IgniteButton
        {
            get => _igniteButton;
            set
            {
                _igniteButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        internal override bool EABBNOODFGL(ShipStatus __instance)
        {
            if (PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected) == 0)
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(
                    PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.ArsonistWin,
                    SendOption.Reliable,
                    -1
                );
                writer.Write(Player.PlayerId);
                Wins();
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                Utils.EndGame();
                return false;
            }

            if (IgniteUsed || Player.Data.IsDead) return true;

            return !CustomGameOptions.ArsonistGameEnd;
        }


        public void Wins()
        {
            //System.Console.WriteLine("Reached Here - Glitch Edition");
            ArsonistWins = true;
        }

        public void Loses()
        {
            Player.Data.IsImpostor = true;
        }

        public bool CheckEveryoneDoused()
        {
            var arsoId = Player.PlayerId;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (
                    player.PlayerId == arsoId ||
                    player.Data.IsDead ||
                    player.Data.Disconnected
                ) continue;
                if (!DousedPlayers.Contains(player.PlayerId)) return false;
            }

            return true;
        }

        protected override void IntroPrefix(IntroCutscene._CoBegin_d__14 __instance)
        {
            var arsonistTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            arsonistTeam.Add(PlayerControl.LocalPlayer);
            __instance.yourTeam = arsonistTeam;
        }
    }
}
