/**
 * VRPGLocalMenu.cs by Toast https://github.com/dorktoast - 11/12/2023
 * VRPG Project Repo: https://github.com/GIBGames/VRPG
 * Join the GIB Games discord at https://discord.gg/gibgames
 * Licensed under MIT: https://opensource.org/license/mit/
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

namespace GIB.VRPG2
{
    /// <summary>
    /// A handler for management of the VRPG Menu
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRPGMenu : VRPGComponent
    {
        [Header("References")]
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private TMP_InputField titleField;
        [SerializeField] private TMP_InputField conditionField;

        [Header("Panels")]
        [SerializeField] private GameObject[] panels;

        private bool menuIsOpen;
        private Vector3 menuStartPosition;
        private Quaternion menuStartRotation;

        [Header("Plates")]
        [SerializeField] private TextMeshProUGUI characterNamePlate;
        [SerializeField] private TextMeshProUGUI characterTitlePlate;
        [SerializeField] private TextMeshProUGUI playerNamePlate;
        [SerializeField] private TextMeshProUGUI playerTitlePlate;
        [SerializeField] private TextMeshProUGUI characterConditionPlate;

        #region Unity/Udon

        private void Start()
        {
            menuStartPosition = transform.position;
            menuStartRotation = transform.rotation;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!player.isLocal) return;

            characterNamePlate.text = "";
            characterTitlePlate.text = "";
            characterConditionPlate.text = "";
            playerNamePlate.text = player.displayName;
            playerTitlePlate.text = "";
        }

        public void SwapMenuState()
        {
            if (menuIsOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        #endregion

        public void OpenMenu()
        {
            menuIsOpen = true;
            PositionMenu();
        }

        public void CloseMenu()
        {
            menuIsOpen = false;
            transform.SetPositionAndRotation(menuStartPosition, menuStartRotation);
        }

        public void PositionMenu()
        {
            Vector3 playerHeadPos = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            Quaternion playerHeadRot = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;

            transform.SetPositionAndRotation(playerHeadPos, playerHeadRot);

            transform.position += transform.forward * .9f;
        }

        public void SetCharacterName(string charName)
        {
            characterNamePlate.text = charName;
        }

        public void SetCharacterTitle(string charTitle)
        {
            characterTitlePlate.text = charTitle;
        }

        public void SetCharacterCondition(string characterCondition)
        {
            characterConditionPlate.text = characterCondition;
        }

        public void SetPlayerTitle(string playerTitle)
        {
            playerTitlePlate.text = playerTitle;
        }

        #region panels

        public void ShowPanel(int index)
        {
            for (int i = 0; i < panels.Length; i++)
            {
                if (i == index)
                {
                    panels[i].SetActive(true);
                }
                else
                {
                    panels[i].SetActive(false);
                }
            }

            panels[index].SetActive(true);
        }

        public void SetNameAndTitle()
        {
            if (Utilities.IsValid(VRPG.LocalPlayerObject))
            {
                VRPG.LocalPlayerObject.SetNameAndTitle(nameField.text, titleField.text, conditionField.text);
                SetPlates(nameField.text, titleField.text, conditionField.text);
            }
        }

        public void SetOOC()
        {
            string oocColorHex = Utils.GetColorHex(VRPG.OocLabelColor);
            if (Utilities.IsValid(VRPG.LocalPlayerObject))
                VRPG.LocalPlayerObject.SetNameAndTitle($"<color=#{oocColorHex}>Out of Character</color>", Networking.LocalPlayer.displayName, "");
        }

        public void SetHidden()
        {
            if (Utilities.IsValid(VRPG.LocalPlayerObject))
                VRPG.LocalPlayerObject.SetNameAndTitle("", "", "");
        }

        public void SetPlates(string newPlateName, string newPlateTitle, string newPlateCondition)
        {
            characterNamePlate.text = newPlateName;
            characterTitlePlate.text = newPlateTitle;
            characterConditionPlate.text = newPlateCondition;
            playerNamePlate.text = Networking.LocalPlayer.displayName;
            playerTitlePlate.text = VRPG.GMData.GetGMTitle(Networking.LocalPlayer.displayName);
        }

        #endregion

    }

}