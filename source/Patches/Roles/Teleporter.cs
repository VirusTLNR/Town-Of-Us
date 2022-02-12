using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using Reactor;
using Reactor.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace TownOfUs.Roles
{
    public class Teleporter : RoleWithCooldown
    {
        private KillButton _teleportButton;

        public Teleporter(PlayerControl player) : base(player, RoleEnum.Teleporter, CustomGameOptions.TeleporterCooldown)
        {
            ImpostorText = () => "Play fifty-two crew pickup";
            TaskText = () => "Play fifty-two crew pickup";
        }

        public KillButton TeleportButton
        {
            get => _teleportButton;
            set
            {
                _teleportButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        public void Teleport()
        {
            ResetCooldownTimer();
            Dictionary<byte, Vector2> coordinates = GenerateTeleportCoordinates();

            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.Teleport,
                SendOption.Reliable,
                -1);
            writer.Write((byte) coordinates.Count);
            foreach ((byte key, Vector2 value) in coordinates)
            {
                writer.Write(key);
                writer.Write(value);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            TeleportPlayersToCoordinates(coordinates);
        }

        public static void TeleportPlayersToCoordinates(Dictionary<byte, Vector2> coordinates)
        {
            if (coordinates.ContainsKey(PlayerControl.LocalPlayer.PlayerId))
            {
                Coroutines.Start(Utils.FlashCoroutine(new Color(0.89f, 0.45f, 0f)));
                if (Minigame.Instance)
                {
                    try
                    {
                        Minigame.Instance.Close();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (PlayerControl.LocalPlayer.inVent)
                {
                    PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                    PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
                }

                // TODO: Do we need CustomRPC.FixAnimation or to set collider or ResetMoveState() or anything like that?
            }


            foreach ((byte key, Vector2 value) in coordinates)
            {
                PlayerControl player = Utils.PlayerById(key);
                player.transform.position = value;
            }
        }

        private Dictionary<byte, Vector2> GenerateTeleportCoordinates()
        {
            // TODO: Do we need to check for PlayerControl.LocalPlayer.Moveble?
            List<PlayerControl> targets = PlayerControl.AllPlayerControls.ToArray()
                .Where(player => CustomGameOptions.TeleportSelf || player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                //.Where(player => !player.Data.IsDead) Time Lord moves around dead players, so I'll be consistent here
                .ToList();

            HashSet<Vent> vents = Object.FindObjectsOfType<Vent>().ToHashSet();

            Dictionary<byte, Vector2> coordinates = new Dictionary<byte, Vector2>(targets.Count);
            foreach (PlayerControl target in targets)
            {
                Vent vent = vents.Random();
                if (!CustomGameOptions.TeleportOccupiedVents)
                {
                    vents.Remove(vent);
                    if (vents.Count == 0)
                    {
                        vents = Object.FindObjectsOfType<Vent>().ToHashSet();
                    }
                }

                Vector3 destination = Utils.GetCoordinatesToSendPlayerToVent(vent);
                coordinates.Add(target.PlayerId, destination);
            }
            return coordinates;
        }
    }
}
