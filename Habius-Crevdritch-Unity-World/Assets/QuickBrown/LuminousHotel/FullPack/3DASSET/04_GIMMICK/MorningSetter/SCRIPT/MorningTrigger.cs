
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MorningTrigger : UdonSharpBehaviour
{
    //MorningSetterを指定
    [SerializeField] public MorningSetter morningSetter;

    //AudioSourceを指定
    [SerializeField] public AudioSource audioSource;
    //AudioClipを指定
    [SerializeField] public AudioClip audioClip_change;
    [SerializeField] public AudioClip audioClip_notChange;

    //Int値でスイッチ用の変数を作成
    [Header("switchValue = 0で朝、1で夜")]
    [Range(0, 1)][SerializeField] public int switchValue = 0;

    //Inspector上にHeaderを追加


    //PickupUseDownで発火するようにする
    public override void OnPickupUseDown()
    {

        if (morningSetter.isChanging == false)
        {
            //audioClip_changeを再生
            audioSource.PlayOneShot(audioClip_change);

            //sendCustomNetworkEventを使って、ChangeAirTimeを発火
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ChangeAirTime");
        }
        else
        {
            //audioClip_notChangeを再生
            audioSource.PlayOneShot(audioClip_notChange);
        }
    }

    //Interact
    public override void Interact()
    {
        if (morningSetter.isChanging == false)
        {
            //audioClip_changeを再生
            audioSource.PlayOneShot(audioClip_change);

            //sendCustomNetworkEventを使って、ChangeAirTimeを発火
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ChangeAirTime");
        }
        else
        {
            //audioClip_notChangeを再生
            audioSource.PlayOneShot(audioClip_notChange);
        }
    }


    public void ChangeAirTime()
    {
        if (switchValue == 0)
        {
            morningSetter.StartMorningChangeProcess();
        }
        else
        {
            morningSetter.StartNightChangeProcess();
        }
    }
}
