using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TownOfUs.Roles;
using TownOfUs.Roles.Modifiers;
using HarmonyLib;

namespace TownOfUs.Patches
{
    [HarmonyPatch]
    static class EndGameSummary
    {
        static class AdditionalTempData
        {
            public static List<PlayerInfo> playerData = new List<PlayerInfo>();

            //leaving this in as i assume it maybe needed in future, feel free to remove if you think otherwise.
            public static void clear()
            {
                playerData.Clear();
            }

            internal class PlayerInfo
            {
                public byte PlayerId { get; set; }
                public string PlayerName { get; set; }
                public Faction Faction { get; set; }
                public Role CurrentRole { get; set; }
                public Modifier Modifier { get; set; }
                public int TasksCompleted { get; set; }
                public int TasksTotal { get; set; }
            }
        }

        public static void UpdatePlayerInfo()
        {
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> players = GameData.Instance.AllPlayers;

            List<AdditionalTempData.PlayerInfo> pinfolist = new List<AdditionalTempData.PlayerInfo>();

            foreach (GameData.PlayerInfo player in players)
            {
                AdditionalTempData.PlayerInfo pinfo = new AdditionalTempData.PlayerInfo();
                byte pid = player.PlayerId;
                pinfo.PlayerName = player.PlayerName;
                pinfo.CurrentRole = Role.GetRole(player);
                pinfo.Faction = pinfo.CurrentRole.Faction;
                pinfo.Modifier = Modifier.GetModifier(player);
                pinfo.TasksCompleted = 0;
                pinfo.TasksTotal = 0;

                foreach (var task in player.Tasks)
                {
                    if (task.Complete == true)
                    {
                        pinfo.TasksCompleted++;
                    }
                    pinfo.TasksTotal++;
                }
                pinfolist.Add(pinfo);
            }
            AdditionalTempData.playerData = pinfolist;
        }

        public static void LoadGameSummary(EndGameManager __instance)
        {
            var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
            GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            roleSummary.transform.position = new Vector3(__instance.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -14f);
            roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

            var roleSummaryText = new StringBuilder();

            roleSummaryText.AppendLine("End-of-game Summary:");

            foreach (var data in AdditionalTempData.playerData)
            {

                //this way works, dont know why the other doesnt tbh.
                var won = "   ";
                foreach (WinningPlayerData winner in TempData.winners)
                {
                    Reactor.PluginSingleton<TownOfUs>.Instance.Log.LogDebug($"WinnerName=#{winner.Name}# vs PlayerName=#{data.PlayerName}#");
                    if (winner.Name == data.PlayerName)
                    {
                        Reactor.PluginSingleton<TownOfUs>.Instance.Log.LogDebug($"TRUE");
                        won = $"[<color=#FAD934FF>W</color>]";
                    }
                }

                //this doesnt work for some reason, it stops both lovers from showing as winners.
                /*var won = "";
                foreach (WinningPlayerData winner in TempData.winners)
                {
                    Reactor.PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"WinnerName=#{winner.Name}# vs PlayerName=#{data.PlayerName}#");
                    if (winner.Name == data.PlayerName)
                    {
                        Reactor.PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"TRUE");
                        won = $"[<color=#FAD934FF>W</color>]";
                    }
                    else
                    {
                        Reactor.PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"FALSE");
                        won = "   ";
                    }
                }*/

                string nameinfo = data.PlayerName;

                List<string> nonCrewRolesWithTasks = new List<string>();

                nonCrewRolesWithTasks.Add("Phantom");

                string taskInfo = ((data.Faction != Faction.Crewmates) && !nonCrewRolesWithTasks.Contains(data.CurrentRole.Name)) ? "" : $" <color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>";

                string roleInfo = "";
                string modifierInfo = "";
                if (data.CurrentRole!=null)
                {
                    roleInfo = $"{data.CurrentRole.ColorString}{data.CurrentRole.Name}</color>";
                }
                if (data.Modifier!=null)
                {
                    modifierInfo = $"{data.Modifier.ColorString}{data.Modifier.Name}</color> ";
                }
                roleSummaryText.AppendLine($"{won}{nameinfo} the {modifierInfo}{roleInfo}{taskInfo}");
            }
            TMPro.TMP_Text roleSummaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
            roleSummaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
            roleSummaryTextMesh.color = Color.white;
            roleSummaryTextMesh.fontSizeMin = 1.5f;
            roleSummaryTextMesh.fontSizeMax = 1.5f;
            roleSummaryTextMesh.fontSize = 1.5f;

            var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
            roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
            roleSummaryTextMesh.text = roleSummaryText.ToString();
        }
    }
}
