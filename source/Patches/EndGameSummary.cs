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

            var pinfolist = new List<AdditionalTempData.PlayerInfo>();

            foreach (GameData.PlayerInfo player in players)
            {
                byte pid = player.PlayerId;
                string name = player.PlayerName;
                Role role = Role.GetRole(player);
                Modifier modifier = Modifier.GetModifier(player);
                Faction faction = role.Faction;
                int tasksdone = 0;
                int taskstotal = 0;

                AdditionalTempData.PlayerInfo pinfo = new AdditionalTempData.PlayerInfo();
                pinfo.PlayerName = name;
                pinfo.CurrentRole = role;
                pinfo.Faction = faction;
                pinfo.Modifier = modifier;

                foreach (var task in player.Tasks)
                {
                        if (task.Complete == true)
                        {
                            tasksdone++;
                        }
                        taskstotal++;
                }
                
                pinfo.TasksCompleted = tasksdone;
                pinfo.TasksTotal = taskstotal;
                pinfolist.Add(pinfo);
            }
            AdditionalTempData.playerData = pinfolist;
        }

        public static bool HasTasks(Faction faction)
        {
            return faction == Faction.Crewmates;
        }

        static bool IsNull(AdditionalTempData.PlayerInfo pinfo, string type)
        {
            return type switch {
                "Role" => pinfo.CurrentRole == null,
                "Mod" => pinfo.Modifier == null,
                _ => true
            };
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
                var won = "   ";
                foreach (var winner in TempData.winners)
                {
                    if (winner.Name == data.PlayerName)
                    {
                        won = $"[<color=#FAD934FF>W</color>]";
                    }
                }
                var nameinfo = data.PlayerName;

                List<string> noncrewroleswithtasks = new List<string>();

                noncrewroleswithtasks.Add("Phantom");

                var taskInfo = (!HasTasks(data.Faction) && !noncrewroleswithtasks.Contains(data.CurrentRole.Name)) ? "" : $" <color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>";

                var roleInfo = "";
                var modifierInfo = "";
                if (!IsNull(data, "Role"))
                {
                    roleInfo = $"{data.CurrentRole.ColorString}{data.CurrentRole.Name}</color>";
                }
                if (!IsNull(data, "Mod"))
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
