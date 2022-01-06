using HarmonyLib;
using Hazel;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnhollowerBaseLib;
using UnityEngine;

namespace TownOfUs.Handshake
{
    public static class ClientHandshake
    {
        //forkname
        public static string forkName = "Anusien";
        public static Dictionary<int, PlayerVersion> playerVersions = new Dictionary<int, PlayerVersion>();
        public static System.Version touVersion = System.Version.Parse(TownOfUs.GetVersion());
        private static float timer = 600f;
        private static float kickingTimer = 0f;
        private static bool versionSent = false;
        private static string lobbyCodeText = "";

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch
        {
            public static void Postfix()
            {
                if (PlayerControl.LocalPlayer != null)
                {
                    shareGameVersion();
                }
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix(GameStartManager __instance)
            {
                // Trigger version refresh
                versionSent = false;
                // Reset lobby countdown timer
                timer = 600f;
                // Reset kicking timer
                kickingTimer = 0f;
                // Copy lobby code
                string code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                GUIUtility.systemCopyBuffer = code;
                lobbyCodeText = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCode, new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "\r\n" + code;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            private static bool update = false;
            private static string currentText = "";

            public static void Prefix(GameStartManager __instance)
            {
                if (!AmongUsClient.Instance.AmHost || !GameData.Instance) return; // Not host or no instance
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }

            public static void Postfix(GameStartManager __instance)
            {
                // Send version as soon as PlayerControl.LocalPlayer exists
                if (PlayerControl.LocalPlayer != null && !versionSent)
                {
                    versionSent = true;
                    shareGameVersion();
                }

                // Host update with version handshake infos
                if (AmongUsClient.Instance.AmHost)
                {
                    bool blockStart = false;
                    string message = "";
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray())
                    {
                        if (client.Character == null) continue;
                        var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                        if (dummyComponent != null && dummyComponent.enabled)
                            continue;
                        else if (!playerVersions.ContainsKey(client.Id))
                        {
                            blockStart = true;
                            message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has a different or no version of Town Of Us\n</color>";
                        }
                        else
                        {
                            PlayerVersion PV = playerVersions[client.Id];
                            int diff = touVersion.CompareTo(PV.version);
                            if (diff > 0)
                            {
                                message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has an older version of Town Of Us (v{playerVersions[client.Id].version.ToString()})\n</color>";
                                blockStart = true;
                            }
                            else if (diff < 0)
                            {
                                message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has a newer version of Town Of Us (v{playerVersions[client.Id].version.ToString()})\n</color>";
                                blockStart = true;
                            }
                            else if (!PV.GuidMatches())
                            { // version presumably matches, check if Guid matches, different fork same version??
                                message += $"<color=#FF0000FF>{client.Character.Data.PlayerName} has a modified version of TOU v{playerVersions[client.Id].version.ToString()} <size=50%>[{forkName}]({PV.guid.ToString()})</size>\n</color>";
                                blockStart = true;
                            }
                        }
                    }
                    if (blockStart)
                    {
                        __instance.StartButton.color = __instance.startLabelText.color = Palette.DisabledClear;
                        __instance.GameStartText.text = message;
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    }
                    else
                    {
                        __instance.StartButton.color = __instance.startLabelText.color = ((__instance.LastPlayerCount >= __instance.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                    }
                }

                // Client update with handshake infos
                if (!AmongUsClient.Instance.AmHost)
                {
                    /*
                    Reactor.PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"##TOUVEERCHECK##:-{touVersion}");
                    Reactor.PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"##TOUHOSTVERCHECK##:-{playerVersions[AmongUsClient.Instance.HostId].version}");
                    foreach (var vers in playerVersions)
                    {
                        Reactor.PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"##vers##:-{vers.Key}//{vers.Value.guid}//{vers.Value.version.ToString()}");
                    }
                    */
                    string message = "";
                    bool triggerUpdate = false;
                    string hostVersion = "";
                    PlayerVersion PV = playerVersions[AmongUsClient.Instance.HostId];
                    int diff = touVersion.CompareTo(PV.version);
                    Reactor.Logger<TownOfUs>.Instance.LogMessage($"MyGUID:- {Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId}");
                    Reactor.Logger<TownOfUs>.Instance.LogMessage($"HostGUID:- {PV.guid}");
                    if (!playerVersions.ContainsKey(AmongUsClient.Instance.HostId) || touVersion.CompareTo(PV.version) != 0)
                    {
                        kickingTimer += Time.deltaTime;
                        if (kickingTimer > 10)
                        {
                            kickingTimer = 0;
                            AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                            SceneChanger.ChangeScene("MainMenu");
                        }
                        //new but not working... needs a change in what is sent via the RPC. for this to work.
                        /*
                        if (!playerVersions.ContainsKey(AmongUsClient.Instance.HostId))
                        {
                            message += $"<color=#FF0000FF>The Host({AmongUsClient.Instance.allClients[AmongUsClient.Instance.HostId].PlayerName}) is not a Town Of Us Host\nYou will be kicked in {Math.Round(10 - kickingTimer)}s</color>";
                        }
                        else if (diff < 0)
                        {
                            message += $"<color=#FF0000FF>The Host({AmongUsClient.Instance.allClients[AmongUsClient.Instance.HostId].PlayerName}) has an older version of Town Of Us (v{PV.version.ToString()})\n</color>";
                            //for linking up with the auto mod update
                            triggerUpdate = true;
                            hostVersion = PV.version.ToString();
                        }
                        else if (diff > 0)
                        {
                            message += $"<color=#FF0000FF>The Host({AmongUsClient.Instance.allClients[AmongUsClient.Instance.HostId].PlayerName}) has a newer version of Town Of Us (v{PV.version.ToString()})\n</color>";
                            //for linking up with the auto mod update
                            triggerUpdate = true;
                            hostVersion = PV.version.ToString();
                        }
                        //else if (!PV.GuidMatches())
                        else if (Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId != PV.guid)
                        {
                            message += $"<color=#FF0000FF>The Host({AmongUsClient.Instance.allClients[AmongUsClient.Instance.HostId].PlayerName}) has a modified version of TOU v{PV.version.ToString()} <size=50%>[{forkName}]({PV.guid.ToString()})</size>\n</color>";
                        }
                        __instance.GameStartText.text = message;
                        */

                        //original
                        __instance.GameStartText.text = $"<color=#FF0000FF>The host has no or a different version of Town Of Us\nYou will be kicked in {Math.Round(10 - kickingTimer)}s</color>";
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                        if (triggerUpdate == true && kickingTimer == 10)
                        {
                            //do auto update --- not implemented yet as the auto update system is not implemented yet.
                            //basically.. after lobby disconnect...
                            //trigger an auto update to save the user having to re-open the game...
                            //the version it should be updating to is 'hostversion'
                            //this will require a change to the RPC
                        }
                    }
                    else
                    {
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                        if (__instance.startState != GameStartManager.StartingStates.Countdown)
                        {
                            __instance.GameStartText.text = String.Empty;
                        }
                    }
                }

                // Lobby code replacement -- not required
                //__instance.GameRoomName.text = TheOtherRolesPlugin.StreamerMode.Value ? $"<color={TheOtherRolesPlugin.StreamerModeReplacementColor.Value}>{TheOtherRolesPlugin.StreamerModeReplacementText.Value}</color>" : lobbyCodeText;

                // Lobby timer
                if (!AmongUsClient.Instance.AmHost || !GameData.Instance) return; // Not host or no instance

                if (update) currentText = __instance.PlayerCounter.text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;
                string suffix = $" ({minutes:00}:{seconds:00})";

                __instance.PlayerCounter.text = currentText + suffix;
                __instance.PlayerCounter.autoSizeTextContainer = true;

            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public class GameStartManagerBeginGame
        {
            public static bool Prefix(GameStartManager __instance)
            {
                // Block game start if not everyone has the same mod version
                bool continueStart = true;

                if (AmongUsClient.Instance.AmHost)
                {
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients)
                    {
                        if (client.Character == null) continue;
                        var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                        if (dummyComponent != null && dummyComponent.enabled)
                            continue;

                        if (!playerVersions.ContainsKey(client.Id))
                        {
                            continueStart = false;
                            break;
                        }

                        PlayerVersion PV = playerVersions[client.Id];
                        int diff = touVersion.CompareTo(PV.version);
                        if (diff != 0 || !PV.GuidMatches())
                        {
                            continueStart = false;
                            break;
                        }
                    }
                }

                //not required
                /*if (CustomOptionHolder.dynamicMap.getBool())
                {
                    // 0 = Skeld
                    // 1 = Mira HQ
                    // 2 = Polus
                    // 3 = Dleks - deactivated
                    // 4 = Airship
                    List<byte> possibleMaps = new List<byte>() { 0, 1, 2, 4 };
                    PlayerControl.GameOptions.MapId = possibleMaps[TheOtherRoles.rnd.Next(possibleMaps.Count)];
                }*/

                return continueStart;
            }
        }

        public class PlayerVersion
        {
            public readonly Version version;
            public readonly Guid guid;

            public PlayerVersion(Version version, Guid guid)
            {
                this.version = version;
                this.guid = guid;
            }

            public bool GuidMatches()
            {
                return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(this.guid);
            }
        }

        public static void shareGameVersion()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionHandshake, Hazel.SendOption.Reliable, -1);
            writer.Write((byte)touVersion.Major);
            writer.Write((byte)touVersion.Minor);
            writer.Write((byte)touVersion.Build);
            writer.WritePacked(AmongUsClient.Instance.ClientId);
            writer.Write((byte)(touVersion.Revision < 0 ? 0xFF : touVersion.Revision));
            writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            versionHandshake(touVersion.Major, touVersion.Minor, touVersion.Build, touVersion.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
        }

        public static void versionHandshake(int major, int minor, int build, int revision, Guid guid, int clientId)
        {
            System.Version ver;
            if (revision < 0)
                ver = new System.Version(major, minor, build);
            else
                ver = new System.Version(major, minor, build, revision);
            playerVersions[clientId] = new PlayerVersion(ver, guid);
        }

        public static void HandshakeRPC(MessageReader reader)
        {
            byte major = reader.ReadByte();
            byte minor = reader.ReadByte();
            byte patch = reader.ReadByte();
            int versionOwnerId = reader.ReadPackedInt32();
            byte revision = 0xFF;
            Guid guid;
            if (reader.Length - reader.Position >= 17)
            { // enough bytes left to read
                revision = reader.ReadByte();
                // GUID
                byte[] gbytes = reader.ReadBytes(16);
                guid = new Guid(gbytes);
            }
            else
            {
                guid = new Guid(new byte[16]);
            }
            Handshake.ClientHandshake.versionHandshake(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
        }
    }
}
