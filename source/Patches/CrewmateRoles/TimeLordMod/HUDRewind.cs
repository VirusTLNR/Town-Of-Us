using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.TimeLordMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public class HUDRewind
    {
        public static void Postfix(PlayerControl __instance)
        {
            UpdateRewindButton(__instance);
        }

        public static void UpdateRewindButton(PlayerControl __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.TimeLord)) return;
            var data = PlayerControl.LocalPlayer.Data;
            var isDead = data.IsDead;
            var rewindButton = DestroyableSingleton<HudManager>.Instance.KillButton;

            var role = Role.GetRole<TimeLord>(PlayerControl.LocalPlayer);

            // The sprite is set in KillButtonSprite
            if (isDead)
            {
                rewindButton.gameObject.SetActive(false);
                // rewindButton.isActive = false;
            }
            else
            {
                rewindButton.gameObject.SetActive(!MeetingHud.Instance);
                rewindButton.isActive = !MeetingHud.Instance; // TODO: I think this is unnecessary?
                if (role.IsRewinding)
                {
                    rewindButton.SetCoolDown(role.TimeRemaining, CustomGameOptions.RewindDuration);
                }
                else
                {
                    rewindButton.SetCoolDown(role.CooldownTimer(), CustomGameOptions.RewindCooldown);
                }
            }

            var renderer = rewindButton.graphic;
            if (
                !rewindButton.isCoolingDown
                && !RecordRewind.rewinding // Other roles show it as enabled when the ability is active, should this?
                && rewindButton.enabled
                )
            {
                renderer.color = Palette.EnabledColor;
                renderer.material.SetFloat("_Desat", 0f);
                return;
            }

            renderer.color = Palette.DisabledClear;
            renderer.material.SetFloat("_Desat", 1f);
        }
    }
}