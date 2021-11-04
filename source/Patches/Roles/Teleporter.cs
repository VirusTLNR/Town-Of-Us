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
    public class Teleporter : Role
    {
        private KillButtonManager _teleportButton;
        private DateTime _lastTeleported;

        public Teleporter(PlayerControl player) : base(player)
        {
            Name = "Teleporter";
            ImpostorText = () => "Play fifty-two crew pickup";
            TaskText = () => "Play fifty-two crew pickup";
            Color = Palette.ImpostorRed;
            RoleType = RoleEnum.Teleporter;
            Faction = Faction.Impostors;
        }

        protected override void DoOnGameStart()
        {
            _lastTeleported = DateTime.UtcNow;
        }

        protected override void DoOnMeetingEnd()
        {
            _lastTeleported = DateTime.UtcNow;
        }

        public KillButtonManager TeleportButton
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
            _lastTeleported = DateTime.UtcNow;
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

                /* On Polus, the game sets the position of the vent as horizontally in the middle of the vent
                 * but vertically at the very bottom. This actually puts the player out of bounds.
                 * So we need to reposition them vertically to be in the middle of the vent.
                 */
                Vector2 size = vent.GetComponent<BoxCollider2D>().size;
                Vector2 destination = vent.transform.position;
                destination.y += size.y / 2;
                coordinates.Add(target.PlayerId, destination);
            }
            return coordinates;
        }

        public float TeleportTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - _lastTeleported;
            var num = CustomGameOptions.TeleporterCooldown * 1000f;
            if (num - (float) timeSpan.TotalMilliseconds < 0f)
            {
                return 0;
            }
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }
    }
}
