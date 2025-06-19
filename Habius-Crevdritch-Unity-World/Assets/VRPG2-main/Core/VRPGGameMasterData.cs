/**
 * VRPGGameMasterData.cs by Toast https://github.com/dorktoast - 11/6/2023
 * VRPG Project Repo: https://github.com/GIBGames/VRPG
 * Join the GIB Games discord at https://discord.gg/gibgames
 * Licensed under MIT: https://opensource.org/license/mit/
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace GIB.VRPG2
{
    public class VRPGGameMasterData : VRPGComponent
    {
        [SerializeField] private GameObject[] GMObjects;
        [SerializeField] private VRC_Pickup[] GMPickups;

        [Header("Titles")]
        public string GameMasterName;
        public string GameMasterTitle;
        public string GameMasterAbv;
        public string GameStaffTitle;
        public string GameStaffAbv;

        [Header("GM Data")]
        private VRCPlayerApi currentGMTemp;
        [SerializeField] private bool useWhitelist = true;
        [SerializeField] private string[] gmNames;

        [Header("GUI")]
        [SerializeField] private VRPGTextElement gmVoiceStatus;
        [SerializeField] private VRPGTextElement gmSpeedStatus;
        [SerializeField] private TMP_Dropdown teleTarget;
        [SerializeField] private AudioSource GMSound;

        [Header("References")]
        public Transform[] TeleportPoints;
        public Transform PlayerTeleTarget;

        [Header("Mod Tools")]
        [SerializeField] private Transform noBox;

        public void OnWhitelistDownloaded()
        {
            ActivateStaff(IsOnStaffList(Networking.LocalPlayer.displayName));
        }

        public bool IsOnStaffList(string userName)
        {
            bool tryStaff = false;
            if (useWhitelist)
                tryStaff = VRPG.Whitelists.IsOnWhitelist("StaffList", userName);

            foreach (string name in gmNames)
            {
                if (userName.ToLower() == name.ToLower())
                {
                    tryStaff = true;
                }
            }

            if (userName.ToLower() == GameMasterName.ToLower()) tryStaff = true;

            return tryStaff;
        }

        public string GetGMIcon(string userName)
        {
            string iconString = "";
            if (userName.ToLower() == GameMasterName.ToLower())
            {
                iconString = $"[{Utils.MakeColor(GameMasterAbv, VRPG.Options.GmColor)}] ";
            }
            else if (IsOnStaffList(userName))
            {
                iconString = $"[{Utils.MakeColor(GameStaffAbv, VRPG.Options.StaffColor)}] ";
            }

            return iconString;
        }

        public string GetGMTitle(string userName)
        {
            string titleString = "";
            if (userName.ToLower() == GameMasterName.ToLower())
            {
                titleString = $"{Utils.MakeColor(GameMasterTitle, VRPG.Options.GmColor)}";
            }
            else if (IsOnStaffList(userName))
            {
                titleString = $"{Utils.MakeColor(GameStaffTitle, VRPG.Options.StaffColor)}";
            }

            return titleString;
        }

        private void ActivateStaff(bool isStaff)
        {
            foreach (GameObject gmObject in GMObjects)
            {
                gmObject.SetActive(isStaff);
            }
        }

        #region Mod tools

        public void AddSelectedToExile()
        {
            if (!Utilities.IsValid(VRPG.Social.SelectedPlayer.Owner)) return;

            VRPG.Whitelists.AddToWhitelist("ExileManifest", VRPG.Social.SelectedPlayer.Owner.displayName);
        }

        public void RemoveSelectedFromExile()
        {
            if (!Utilities.IsValid(VRPG.Social.SelectedPlayer.Owner)) return;

            VRPG.Whitelists.RemoveFromWhitelist("ExileManifest", VRPG.Social.SelectedPlayer.Owner.displayName);
        }

        private void ExileMe()
        {
            Networking.LocalPlayer.TeleportTo(noBox.position, noBox.rotation);
        }

        private void CheckForExile()
        {
            if (VRPG.Whitelists.IsOnWhitelist("ExileManifest", Networking.LocalPlayer.displayName))
            {
                ExileMe();
            }
        }

        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (player.isLocal)
                CheckForExile();
        }

        public void CallST()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncCallST");
            VRPG.Logger.LogGMRaw($"{Networking.LocalPlayer.displayName} called for an ST!");
        }

        public void SyncCallST()
        {
            if(IsOnStaffList(Networking.LocalPlayer.displayName))
            {
                GMSound.Play();
            }
        }

        #endregion

        #region ST Teleport

        public void TeleToSelected()
        {
            Debug.Log("Executing TeleToSelected");
            if (PlayerTeleTarget == null)
            {
                VRPG.Logger.DebugLog("PlayerTeleTarget is not assigned, cannot teleport to players.", gameObject);
                return;
            }

            if (VRPG.Social.SelectedPlayer != null)
            {
                VRCPlayerApi targetPlayer = VRPG.Social.SelectedPlayer.Owner;

                PlayerTeleTarget.SetPositionAndRotation(targetPlayer.GetPosition(), targetPlayer.GetRotation());

                // Calculate position 1 meter in front of the target
                Vector3 desiredPosition = PlayerTeleTarget.position + (PlayerTeleTarget.forward * 1.5f);
                PlayerTeleTarget.position = desiredPosition;

                // do rotation
                Vector3 directionToTarget = targetPlayer.GetPosition() - PlayerTeleTarget.position;
                Quaternion desiredRotation = Quaternion.LookRotation(directionToTarget);
                PlayerTeleTarget.rotation = desiredRotation;

                Networking.LocalPlayer.TeleportTo(PlayerTeleTarget.position, PlayerTeleTarget.rotation);
            }
            else
            {
                Debug.Log("Selected Player was null!");
            }
        }

        public void TrySpawn0()
        {
            if (teleTarget.value == 0)
            {
                GoSpawn0();
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "GoSpawn0");
                GoSpawn0();
            }
        }

        public void TrySpawn1()
        {
            if (teleTarget.value == 0)
            {
                GoSpawn1();
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "GoSpawn1");
                GoSpawn1();
            }
        }

        public void TrySpawn2()
        {
            if (teleTarget.value == 0)
            {
                GoSpawn2();
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "GoSpawn2");
                GoSpawn2();
            }
        }

        public void TrySpawn3()
        {
            if (teleTarget.value == 0)
            {
                GoSpawn3();
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "GoSpawn3");
                GoSpawn3();
            }
        }

        public void TrySpawn4()
        {
            if (teleTarget.value == 0)
            {
                GoSpawn4();
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "GoSpawn4");
                GoSpawn4();
            }
        }

        public void TrySpawn5()
        {
            if (teleTarget.value == 0)
            {
                GoSpawn5();
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "GoSpawn5");
                GoSpawn5();
            }
        }

        public void GoSpawn0()
        {
            VRPG.Teleporter.TeleportPlayer(TeleportPoints[0].position, TeleportPoints[0].rotation);
        }

        public void GoSpawn1()
        {
            VRPG.Teleporter.TeleportPlayer(TeleportPoints[1].position, TeleportPoints[1].rotation);
        }

        public void GoSpawn2()
        {
            VRPG.Teleporter.TeleportPlayer(TeleportPoints[2].position, TeleportPoints[2].rotation);
        }

        public void GoSpawn3()
        {
            VRPG.Teleporter.TeleportPlayer(TeleportPoints[3].position, TeleportPoints[3].rotation);
        }

        public void GoSpawn4()
        {
            VRPG.Teleporter.TeleportPlayer(TeleportPoints[4].position, TeleportPoints[4].rotation);
        }

        public void GoSpawn5()
        {
            VRPG.Teleporter.TeleportPlayer(TeleportPoints[5].position, TeleportPoints[5].rotation);
        }

        #endregion

        #region ST Voice

        public void STVoiceOn()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncSTVoiceOn");
        }

        public void STVoiceOff()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncSTVoiceOff");
        }

        public void SyncSTVoiceOn()
        {
            UpdateCurrentST();
            if (gmVoiceStatus)
                gmVoiceStatus.Text = "<color=\"red\">ON</color>";
            currentGMTemp.SetVoiceDistanceNear(9999);
            currentGMTemp.SetVoiceDistanceFar(10000);
        }

        public void SyncSTVoiceOff()
        {
            UpdateCurrentST();
            if (gmVoiceStatus)
                gmVoiceStatus.Text = "OFF";
            currentGMTemp.SetVoiceDistanceNear(0);
            currentGMTemp.SetVoiceDistanceFar(25);
        }

        #endregion

        #region ST Speed

        public void STSpeedOn()
        {
            if (gmSpeedStatus)
                gmSpeedStatus.Text = "<color=\"red\">ON</color>";
            Networking.LocalPlayer.SetRunSpeed(10f);
        }

        public void STSpeedOff()
        {
            if (gmSpeedStatus)
                gmSpeedStatus.Text = "OFF";
            Networking.LocalPlayer.SetRunSpeed(4f);
        }

        #endregion

        public void UpdateCurrentST()
        {
            currentGMTemp = Networking.GetOwner(gameObject);
        }
    }
}