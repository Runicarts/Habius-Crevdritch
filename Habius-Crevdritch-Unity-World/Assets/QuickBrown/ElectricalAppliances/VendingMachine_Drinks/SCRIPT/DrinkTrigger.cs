using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DrinkTrigger : UdonSharpBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip openSound;

    [SerializeField]
    private AudioClip touchSound;

    [SerializeField]
    private AudioClip releaseSound;

    [SerializeField]
    private AudioClip deleteSound;

    [SerializeField]
    private float pitchmin = 0.7f;

    [SerializeField]
    private float pitchmax = 1.4f;

    private bool isOpened = false;
    private float time = 0;

    [SerializeField]
    private float reOpenTime = 3.0f;

    [SerializeField]
    private bool isOpenSound = false;

    [SerializeField]
    private bool isPickupSound = false;

    [SerializeField]
    private VRC_Pickup pickup;

    //pickup
    public override void OnPickup()
    {
        if (isPickupSound)
        {
            //audioSourceのピッチを毎回変えることで、同じ音でも毎回違う音に聞こえるようにする
            audioSource.pitch = Random.Range(pitchmin, pitchmax);
            audioSource.PlayOneShot(touchSound);
        }
    }

    //drop
    public override void OnDrop()
    {
        if (isPickupSound)
        {
            //audioSourceのピッチを毎回変えることで、同じ音でも毎回違う音に聞こえるようにする
            audioSource.pitch = Random.Range(pitchmin, pitchmax);
            audioSource.PlayOneShot(releaseSound);
        }
    }

    //enableになったら
    public void OnEnable()
    {
        isOpened = false;
    }

    //pickupUseDown
    public override void OnPickupUseDown()
    {
        if (!isOpened)
        {
            if (isOpenSound)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Open");
                isOpened = true;
            }
        }
    }

    //Update
    public void Update()
    {
        //isOpenがTrueのとき、５秒後にisOpenをfalseにし、再度開けられるようにする
        if (isOpened)
        {
            time += Time.deltaTime;
            if (time > reOpenTime)
            {
                isOpened = false;
                time = 0;
            }
        }
    }

    public void Open()
    {
        //audioSourceのピッチを毎回変えることで、同じ音でも毎回違う音に聞こえるようにする
        audioSource.pitch = Random.Range(pitchmin, pitchmax);
        audioSource.PlayOneShot(openSound);
    }

    //指定した名前のトリガーに接触したらDeleteDrinkを呼ぶ
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "DrinkDeleter")
        {
            //audioSourceのピッチを毎回変えることで、同じ音でも毎回違う音に聞こえるようにする
            audioSource.pitch = Random.Range(pitchmin, pitchmax);

            SendCustomNetworkEvent(
                VRC.Udon.Common.Interfaces.NetworkEventTarget.All,
                "DeleteDrink"
            );
        }
    }

    public void DeleteDrink()
    {
        if (deleteSound != null)
        {
            audioSource.PlayOneShot(deleteSound);
        }

        //自分を非アクティブにする
        this.gameObject.SetActive(false);

        //pickupを解除する
        if (pickup != null)
        {
            pickup.Drop();
        }
    }
}
