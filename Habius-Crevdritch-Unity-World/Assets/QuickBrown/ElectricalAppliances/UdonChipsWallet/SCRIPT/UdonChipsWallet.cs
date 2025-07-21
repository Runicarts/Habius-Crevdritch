using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace UCS
{
    public class UdonChipsWallet : UdonSharpBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textMeshPro = null;

        [SerializeField]
        private Text text = null;

        [SerializeField]
        private string format = string.Empty;

        [SerializeField]
        private bool dramRoll = false;

        [SerializeField]
        private float dramRollMinPerSec = 10;

        [SerializeField]
        private float dramRollFactorPerSec = 1f;

        float lastMoney = 0f;
        bool firstTake = true;

        [Header("UdonChipsを設定してください（Questでは必須）\n/Set up UdonChips (required by Quest)")]
        [SerializeField]
        UdonChips udonChips;

        private void Start()
        {
            if (textMeshPro == null)
            {
                textMeshPro = GetComponent<TextMeshProUGUI>();
            }

            if (text == null)
            {
                text = GetComponent<Text>();
            }

            if (udonChips == null)
            {
                GameObject udonChipsObject = GameObject.Find("UdonChips");
                if (udonChipsObject != null)
                {
                    udonChips = udonChipsObject.GetComponent<UdonChips>();
                    textMeshPro.text = "Error";
                    Debug.LogError("UdonChipsが見つかりませんでした。手動設定してください。");
                }
            }
        }

        private void OnEnable()
        {
            firstTake = true;
        }

        private void Update()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            if (firstTake)
            {
                firstTake = false;
                lastMoney = udonChips.money;
                ApplyText();
            }

            // 変更があったときだけ表示を変える
            if (lastMoney != udonChips.money)
            {
                if (dramRoll)
                {
                    float delta = lastMoney - udonChips.money;

                    float maxDelta =
                        Mathf.Max(dramRollMinPerSec, Mathf.Abs(delta * dramRollFactorPerSec))
                        * Time.deltaTime;
                    lastMoney = Mathf.MoveTowards(lastMoney, udonChips.money, maxDelta);
                }
                else
                {
                    lastMoney = udonChips.money;
                }
                ApplyText();
            }
        }

        private void ApplyText()
        {
            if (string.IsNullOrEmpty(format))
            {
                format = udonChips.format;
            }

            if (text != null)
            {
                text.text = string.Format(format, lastMoney);
            }
            if (textMeshPro != null)
            {
                textMeshPro.text = string.Format(format, lastMoney);
            }
        }
    }
}
