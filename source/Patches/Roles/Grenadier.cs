using System;
using UnityEngine;
using System.Linq;

namespace TownOfUs.Roles
{
    public class Grenadier : Role
    {
        private KillButtonManager _flashButton;
        public bool Enabled;
        private DateTime _lastFlashed;
        public float TimeRemaining;
        private static Il2CppSystem.Collections.Generic.List<PlayerControl> _closestPlayers;

        public Grenadier(PlayerControl player) : base(player, RoleEnum.Grenadier)
        {
            ImpostorText = () => "Hinder the Crewmates Vision";
            TaskText = () => "Blind the crewmates to get sneaky kills";
        }

        protected override void DoOnGameStart()
        {
            _lastFlashed = DateTime.UtcNow;
            TimeRemaining = 0f;
        }

        protected override void DoOnMeetingEnd()
        {
            _lastFlashed = DateTime.UtcNow;
            TimeRemaining = 0f;
        }

        public bool Flashed => TimeRemaining > 0f;


        public KillButtonManager FlashButton
        {
            get => _flashButton;
            set
            {
                _flashButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        public float FlashTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - _lastFlashed;
            var num = CustomGameOptions.GrenadeCooldown * 1000f;
            var flag2 = num - (float)timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float)timeSpan.TotalMilliseconds) / 1000f;
        }

        public void Flash()
        {
            if (Enabled != true)
            {
                _closestPlayers = FindClosestPlayers(Player);
            }
            Enabled = true;
            TimeRemaining -= Time.deltaTime;

            bool sabActive = Utils.IsSabotageActive();

            foreach (var player in _closestPlayers)
            {
                if (PlayerControl.LocalPlayer.PlayerId != player.PlayerId)
                {
                    continue;
                }

                // TODO: Hoist `!sabActive`
                if (TimeRemaining > CustomGameOptions.GrenadeDuration - 0.5f && !sabActive)
                {
                    float fade = (TimeRemaining - CustomGameOptions.GrenadeDuration) * -2.0f;
                    if (!player.Data.IsImpostor && !player.Data.IsDead && !MeetingHud.Instance)
                    {
                        ((Renderer)DestroyableSingleton<HudManager>.Instance.FullScreen).enabled = true;
                        DestroyableSingleton<HudManager>.Instance.FullScreen.color = Color.Lerp((new Color(0.83f, 0.83f, 0.83f, 0f)), (new Color(0.83f, 0.83f, 0.83f, 1f)), fade);
                    }
                    else if ((player.Data.IsImpostor || player.Data.IsDead) && !MeetingHud.Instance)
                    {
                        ((Renderer)DestroyableSingleton<HudManager>.Instance.FullScreen).enabled = true;
                        DestroyableSingleton<HudManager>.Instance.FullScreen.color = Color.Lerp((new Color(0.83f, 0.83f, 0.83f, 0f)), (new Color(0.83f, 0.83f, 0.83f, 0.2f)), fade);
                        if (PlayerControl.LocalPlayer.Data.IsImpostor && MapBehaviour.Instance.infectedOverlay.SabSystem.Timer < 0.5f)
                        {
                            MapBehaviour.Instance.infectedOverlay.SabSystem.Timer = 0.5f;
                        }
                    }
                    else
                    {
                        ((Renderer)DestroyableSingleton<HudManager>.Instance.FullScreen).enabled = true;
                        DestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0.83f, 0.83f, 0.83f, 0f);
                    }
                }
                else if (TimeRemaining <= (CustomGameOptions.GrenadeDuration - 0.5f) && TimeRemaining >= 0.5f && !sabActive)
                {
                    if ((!player.Data.IsImpostor && !player.Data.IsDead) && !MeetingHud.Instance)
                    {
                        ((Renderer)DestroyableSingleton<HudManager>.Instance.FullScreen).enabled = true;
                        DestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0.83f, 0.83f, 0.83f, 1f);
                    }
                    else if ((player.Data.IsImpostor || player.Data.IsDead) && !MeetingHud.Instance)
                    {
                        ((Renderer)DestroyableSingleton<HudManager>.Instance.FullScreen).enabled = true;
                        DestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0.83f, 0.83f, 0.83f, 0.2f);
                        if (PlayerControl.LocalPlayer.Data.IsImpostor && MapBehaviour.Instance.infectedOverlay.SabSystem.Timer < 0.5f)
                        {
                            MapBehaviour.Instance.infectedOverlay.SabSystem.Timer = 0.5f;
                        }
                    }
                    else
                    {
                        ((Renderer)DestroyableSingleton<HudManager>.Instance.FullScreen).enabled = true;
                        DestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0.83f, 0.83f, 0.83f, 0f);
                    }
                }
                else if (TimeRemaining < 0.5f && !sabActive)
                {
                    float fade2 = (TimeRemaining * -2.0f) + 1.0f;
                    if ((!player.Data.IsImpostor && !player.Data.IsDead) && !MeetingHud.Instance)
                    {
                        ((Renderer)DestroyableSingleton<HudManager>.Instance.FullScreen).enabled = true;
                        DestroyableSingleton<HudManager>.Instance.FullScreen.color = Color.Lerp((new Color(0.83f, 0.83f, 0.83f, 1f)), (new Color(0.83f, 0.83f, 0.83f, 0f)), fade2);
                    }
                    else if ((player.Data.IsImpostor || player.Data.IsDead) && !MeetingHud.Instance)
                    {
                        ((Renderer)DestroyableSingleton<HudManager>.Instance.FullScreen).enabled = true;
                        DestroyableSingleton<HudManager>.Instance.FullScreen.color = Color.Lerp((new Color(0.83f, 0.83f, 0.83f, 0.2f)), (new Color(0.83f, 0.83f, 0.83f, 0f)), fade2);
                        if (PlayerControl.LocalPlayer.Data.IsImpostor && MapBehaviour.Instance.infectedOverlay.SabSystem.Timer < 0.5f)
                        {
                            MapBehaviour.Instance.infectedOverlay.SabSystem.Timer = 0.5f;
                        }
                    }
                    else
                    {
                        ((Renderer)DestroyableSingleton<HudManager>.Instance.FullScreen).enabled = true;
                        DestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0.83f, 0.83f, 0.83f, 0f);
                    }
                }
                else
                {
                    ((Renderer)DestroyableSingleton<HudManager>.Instance.FullScreen).enabled = true;
                    DestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0.83f, 0.83f, 0.83f, 0f);
                    TimeRemaining = 0.0f;
                }
            }

            if (TimeRemaining > 0.5f)
            {
                if (PlayerControl.LocalPlayer.Data.IsImpostor && MapBehaviour.Instance.infectedOverlay.SabSystem.Timer < 0.5f)
                {
                    MapBehaviour.Instance.infectedOverlay.SabSystem.Timer = 0.5f;
                }
            }
        }

        public void UnFlash()
        {
            Enabled = false;
            _lastFlashed = DateTime.UtcNow;
            ((Renderer)DestroyableSingleton<HudManager>.Instance.FullScreen).enabled = true;
            DestroyableSingleton<HudManager>.Instance.FullScreen.color = new Color(0.83f, 0.83f, 0.83f, 0f);
        }

        public static Il2CppSystem.Collections.Generic.List<PlayerControl> FindClosestPlayers(PlayerControl player)
        {
            Il2CppSystem.Collections.Generic.List<PlayerControl> playerControlList = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            float impostorLightMod = PlayerControl.GameOptions.ImpostorLightMod;
            Vector2 truePosition = player.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int index = 0; index < allPlayers.Count; ++index)
            {
                GameData.PlayerInfo playerInfo = allPlayers[index];
                if (!playerInfo.Disconnected)
                {
                    Vector2 vector2 = new Vector2(playerInfo.Object.GetTruePosition().x - truePosition.x, playerInfo.Object.GetTruePosition().y - truePosition.y);
                    float magnitude = ((Vector2) vector2).magnitude;
                    if (magnitude <= impostorLightMod * 5)
                    {
                        PlayerControl playerControl = playerInfo.Object;
                        playerControlList.Add(playerControl);
                    }
                }
            }
            return playerControlList;
        }
    }
}