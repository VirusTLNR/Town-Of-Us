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
            Coroutines.Start(Utils.FlashCoroutine(new Color(0.89f, 0.45f, 0f)));
            foreach ((byte key, Vector2 value) in coordinates)
            {
                PlayerControl player = Utils.PlayerById(key);
                player.transform.position = value;
            }
        }

        private Dictionary<byte, Vector2> GenerateTeleportCoordinates()
        {
            List<PlayerControl> targets = PlayerControl.AllPlayerControls.ToArray()
                .Where(player => player.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                .Where(player => !player.Data.IsDead)
                .ToList();

            List<Vent> vents = Object.FindObjectsOfType<Vent>().ToList();

            Dictionary<byte, Vector2> coordinates = new Dictionary<byte, Vector2>(targets.Count);
            foreach (PlayerControl target in targets)
            {
                Vent destination = vents.Random();
                coordinates.Add(target.PlayerId, destination.transform.position);
            }

            return coordinates;
        }

        public float TeleportTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - _lastTeleported;
            var num = CustomGameOptions.TeleporterCooldown * 1000f;
            var flag2 = num - (float) timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }
    }
}