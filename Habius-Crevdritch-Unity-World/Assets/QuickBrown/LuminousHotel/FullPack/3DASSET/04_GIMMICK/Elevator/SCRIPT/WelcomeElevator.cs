
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WelcomeElevator : UdonSharpBehaviour
{
    [SerializeField] private GameObject elevatorParent;
    [SerializeField] private AudioSource audioSource_Open;
    [SerializeField] private AudioSource audioSource_Close;
    [SerializeField] private AudioSource audioSource_Tone;
    [SerializeField] private Animator animator;
    [SerializeField] private float closeDistance = 4.0f;
    [SerializeField] private float JoinAreaThreshold = 0.5f;
    [SerializeField] GameObject spawnPoint;
    private VRCPlayerApi lastRespawnedPlayer = null;


    public bool doorIsOpen = false;

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
        //スポーン時の距離を取得
        if (Vector3.Distance(spawnPoint.transform.position, player.GetPosition()) <= JoinAreaThreshold)
        {
            lastRespawnedPlayer = player;

            //ドアが閉まっている場合
            if (!doorIsOpen)
            {
                OpenNetwork();
                ToneAudio();
            }
            else
            {
                ToneAudio();
            }
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        //スポーン時の距離を取得
        if (Vector3.Distance(spawnPoint.transform.position, player.GetPosition()) <= JoinAreaThreshold)
        {
            ToneAudioNetwork();
            OpenNetwork();
            LampBlinkNetwork();
        }
    }

    


    private void Update()
    {
        if (doorIsOpen)
        {
            bool allPlayersFarEnough = true;
            VRCPlayerApi[] players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);
            foreach (VRCPlayerApi player in players)
            {
                if (player != null && Vector3.Distance(transform.position, player.GetPosition()) <= closeDistance)
                {
                    allPlayersFarEnough = false;
                    break;
                }
            }

            // リスポーンしたプレイヤーがまだ近くにいるかどうかをチェック
            if (lastRespawnedPlayer != null && Vector3.Distance(transform.position, lastRespawnedPlayer.GetPosition()) <= closeDistance)
            {
                allPlayersFarEnough = false;
            }

            if (allPlayersFarEnough)
            {
                Close();
                doorIsOpen = false;
                lastRespawnedPlayer = null; // プレイヤーが離れたのでリセット
            }
        }
    }

    public void Open()
    {
        if (!doorIsOpen)
        {
            if (animator != null)
            {
                animator.SetTrigger("OpenTrigger");
            }

            if (audioSource_Open != null)
            {
                audioSource_Open.Play();
            }
        }
        doorIsOpen = true;
    }


    public void OpenNetwork()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Open");
    } 
    
    public void ToneAudioNetwork()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ToneAudio");
    }
    
    public void LampBlinkNetwork()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LampBlink");
    }

    public void Close()
    {
        if (animator != null)
        {
            animator.SetTrigger("CloseTrigger");
        }

        if (audioSource_Close != null)
        {
            audioSource_Close.Play();
        }
    }

    private void FadeOut()
    {
        animator.SetTrigger("FadeOutTrigger");
    }
    private void FadeIn()
    {
        animator.SetTrigger("FadeInTrigger");
    }

    public void LampBlink()
    {
        animator.SetTrigger("LampBlinkTrigger");
    }

    public void ToneAudio()
    {
        if (audioSource_Tone != null)
        {
            audioSource_Tone.Play();
        }
    }
}
