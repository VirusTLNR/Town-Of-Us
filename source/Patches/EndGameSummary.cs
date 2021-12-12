using System;
using System.Collections.Generic;
//using Il2CppSystem.Collections.Generic;
using System.Text;
using UnityEngine;
using Reactor;
using TownOfUs.Roles;
using TownOfUs.Roles.Modifiers;
using System.Linq;
using HarmonyLib;

namespace TownOfUs.Patches
{
    [HarmonyPatch]
    static class EndGameSummary
    {
        static class AdditionalTempData
        {
            public static List<PlayerInfo> playerData = new List<PlayerInfo>();

            public static void clear()
            {
                playerData.Clear();
            }

            internal class PlayerInfo
            {
                public int PlayerId { get; set; }
                public string PlayerName { get; set; }
                public Faction Faction { get; set; }
                public Role CurrentRole { get; set; }
                public Modifier Modifier { get; set; }
                public int TasksCompleted { get; set; }
                public int TasksTotal { get; set; }
            }
        }

        public static System.Collections.Generic.List<PlayerControl> allPlayers = new System.Collections.Generic.List<PlayerControl>();
        public static bool playerInfoUpdateFlag1Updating = false;
        public static bool playerInfoUpdateFlag2EndOfGame = false;

        public static void UpdatePlayerDataRPCCall()
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
        (byte)CustomRPC.UpdateGamePlayerControlData, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void GatherPlayerData(byte rpccallid)
        {
            EndGameSummary.allPlayers.Clear();
            EndGameSummary.allPlayers = EndGameSummary.GetAllPlayers();
            List<PlayerControl> allplayersList = EndGameSummary.allPlayers;
            UpdatePlayerInfo(allplayersList);
        }

        public static void UpdatePlayerInfo(List<PlayerControl> players)
        {
            var pinfolist = new List<AdditionalTempData.PlayerInfo>();

            foreach (PlayerControl player in players)
            {
                int pid = player.PlayerId;
                string name = player.name;
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

                PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"PlayerName:- " + player.name);
                foreach (var task in player.myTasks)
                {
                    var firstText = task.name;//.Cast<ImportantTextTask>();
                    PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"TaskName:- " + task.name);
                    var modname = "junktonull";
                    var rolename = "junktonull";
                    if (!IsNull(pinfo, "Mod"))
                        modname = modifier.Name;

                    if (!IsNull(pinfo, "Role"))
                        rolename = role.Name;

                    if (!firstText.Contains(modifier.Name) && !firstText.Contains(role.Name) && !firstText.Contains("_Player"))
                    {
                        if (task.IsComplete == true)
                        {
                            tasksdone++;
                        }
                        taskstotal++;
                    }
                }

                PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"TaskDone:- " + tasksdone);
                PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"TaskTotal:- " + taskstotal);
                pinfo.TasksCompleted = tasksdone;
                pinfo.TasksTotal = taskstotal;
                pinfolist.Add(pinfo);
                tasksdone = 0;
                taskstotal = 0;
                AdditionalTempData.playerData = pinfolist;
            }
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


                var taskInfo = "";//GameData.Instance.AllPlayers[data.PlayerId].PlayerName;
                                  //if (HasTasks(data.Faction))
                                  //{
                taskInfo = data.Faction > 0 ? $"" : $" <color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>";
                //}

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
            roleSummaryTextMesh.faceColor = Color.gray;
            roleSummaryTextMesh.fontSizeMin = 1.5f;
            roleSummaryTextMesh.fontSizeMax = 1.5f;
            roleSummaryTextMesh.fontSize = 1.5f;

            var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
            roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
            roleSummaryTextMesh.text = roleSummaryText.ToString();
        }
       

        public static List<PlayerControl> GetAllPlayers()
        {
            return PlayerControl.AllPlayerControls.ToArray().ToList();
        }
    }
}
