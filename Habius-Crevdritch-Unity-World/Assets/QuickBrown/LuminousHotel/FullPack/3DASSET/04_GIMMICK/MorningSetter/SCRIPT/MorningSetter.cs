using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class MorningSetter : UdonSharpBehaviour
{
    public Color dayFogColor = new Color(0.89f, 0.85f, 0.82f, 1f);
    public Color nightFogColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    public Color targetFogColor;
    private Color initialFogColor;
    public bool isChanging = false;
    public float changeDuration = 20f;
    private float changeTimer = 0f;
    public int morningHour = 4;
    public int morningMinutes = 0;

    [UdonSynced] public int masterTimeHour = 0;
    [UdonSynced] public int masterTimeMinutes = 0;


    [SerializeField] public Animator animator;

    public bool isMorning = false;
    private DateTime lastCheckedTime = DateTime.MinValue;



    //textmeshproを使う
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private TextMeshProUGUI morningTimeText;


    private void Start()
    {
        initialFogColor = RenderSettings.fogColor;

        morningTimeText.text = morningHour.ToString("00") + ":" + morningMinutes.ToString("00");

        CheckTimeAndUpdate();
    }

    private void Update()
    {
        if ((DateTime.Now - lastCheckedTime).TotalMinutes >= 1)
        {
            lastCheckedTime = DateTime.Now;
            CheckTimeAndUpdate();
        }

        if (isChanging)
        {
            changeTimer += Time.deltaTime;

            if (changeTimer >= changeDuration)
            {
                FinishChangeProcess();
            }
            else
            {
                UpdateFogColor();
            }
        }
    }

    private void CheckTimeAndUpdate()
    {
        //MasterClientのみが処理を行う
        if (!Networking.IsMaster)
        {
            return;
        }

        DateTime currentTime = DateTime.Now;


        //Masterの時間を同期
        masterTimeHour = currentTime.Hour;
        masterTimeMinutes = currentTime.Minute;

        //textmeshproのnullチェック
        MasterTimeTextDisplay();

        RequestSerialization();

        bool isMorningTime = (currentTime.Hour == morningHour && (currentTime.Minute == morningMinutes || currentTime.Minute == (morningMinutes + 1) % 60));

        //warningのログを出す
        Debug.LogWarning("分" + currentTime.Minute);
        Debug.LogWarning("時間" + currentTime.Hour);


        // morningMinutesが59の場合、次の時間の0分も許容する
        if (morningMinutes == 59)
        {
            isMorningTime = isMorningTime || (currentTime.Hour == (morningHour + 1) % 24 && currentTime.Minute == 0);
        }

        if (!isChanging && isMorningTime && !isMorning)
        {
            Debug.LogWarning("チェンジスタート " + currentTime.Hour + ":" + currentTime.Minute);

            //StartMorningChangeProcess()をSendCustomNetworkEventで全員に送信
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StartMorningChangeProcess");
        }
    }


    private void MasterTimeTextDisplay()
    {
        //textmeshproのnullチェック
        if (dateText != null)
        {
            dateText.text = masterTimeHour.ToString("00") + ":" + masterTimeMinutes.ToString("00");
        }
    }

    //同期させる
    public override void OnDeserialization()
    {
        MasterTimeTextDisplay();
    }


    private void FinishChangeProcess()
    {
        isChanging = false;
        changeTimer = 0f;
    }

    private void UpdateFogColor()
    {
        float lerpFactor = changeTimer / changeDuration;
        RenderSettings.fogColor = Color.Lerp(initialFogColor, targetFogColor, lerpFactor);
    }

    public void StartMorningChangeProcess()
    {
        isMorning = true;
        initialFogColor = RenderSettings.fogColor;
        isChanging = true;
        changeTimer = 0f;
        targetFogColor = dayFogColor;
        animator.SetBool("isMorning", true);
        animator.SetTrigger("changeTrigger");
    }

    public void StartNightChangeProcess()
    {
        isMorning = false;
        initialFogColor = RenderSettings.fogColor;
        isChanging = true;
        changeTimer = 0f;
        targetFogColor = nightFogColor;
        animator.SetBool("isMorning", false);
        animator.SetTrigger("changeTrigger");
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        //MasterClientが処理を送信
        if (Networking.IsMaster)
        {
            if (!isChanging)
            {
                if (isMorning)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetImmediateMorning");
                }
                else
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetImmediateNight");
                }
            }
        }
    }

    private void SetImmediateState(Color fogColor, bool morning)
    {
        RenderSettings.fogColor = fogColor;

        if (morning)
        {
            animator.SetTrigger("isMorningImmediateTrigger");
        }
        else
        {
            animator.SetTrigger("isNightImmediateTrigger");
        }
    }

    public void SetImmediateMorning()
    {
        SetImmediateState(dayFogColor, true);
    }

    public void SetImmediateNight()
    {
        SetImmediateState(nightFogColor, false);
    }
}
