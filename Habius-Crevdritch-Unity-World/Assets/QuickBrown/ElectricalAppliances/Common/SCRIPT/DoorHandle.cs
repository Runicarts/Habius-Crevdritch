using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public enum DoorAxis
{
    X,
    Y,
    Z
}

public class DoorHandle : UdonSharpBehaviour
{
    // 音関連のフィールド
    [Header("音")]
    public AudioSource audioSource; // オーディオソース
    public AudioClip pickupSound; // ピックアップ音
    public AudioClip openSound; // 開く音
    public AudioClip closeSound; // 閉じる音

    // ドア関連のフィールド
    [Header("ドア")]
    public Transform handleObject; // ドアのハンドル
    public Transform doorObject; // ドアのオブジェクト
    public Rigidbody doorRigidbody; // ドアのリジッドボディ
    public Vector3 initialPosition; // ドアの初期位置
    public float maxDistance = 1.0f; // ドアが開く距離
    public float closeThreshold = 1.0f; // ドアが閉じる角度
    public float doorCloseDelay = 1.0f; // ドアが閉じるまでの遅延時間
    private float doorOpenedTime; // ドアが開いた時間
    public VRC_Pickup pickup;

    // ドアの状態フラグ
    [Header("ドアの状態フラグ")]
    public bool isPickedUp = false; //ハンドルを持っているかどうか
    public bool isOpened = false; // ドアが開いているかどうか
    public bool isClosing = false; // ドアが閉じているかどうか

    // アニメーター
    [Header("アニメーター")]
    public Animator animator;

    [Header("ドアの軸")]
    public DoorAxis doorAxis = DoorAxis.Y; // Inspectorで設定できるようにする

    void Start()
    {
        initialPosition = handleObject.position;
        pickup = GetComponent<VRC_Pickup>(); // VRCPickup コンポーネントを取得
    }

    public override void OnPickup()
    {
        Networking.SetOwner(Networking.LocalPlayer, doorObject.gameObject);
        isPickedUp = true;
        PlaySound(pickupSound);
    }

    public override void OnDrop()
    {
        isPickedUp = false;
    }

    void Update()
    {
        if (isPickedUp)
        {
            HandlePickup();
        }
        else if (isClosing)
        {
            ResetDoorFlags();
        }
    }

    private void HandlePickup()
    {
        if (!isOpened && IsHandlePulled()) // ドアが開いていない状態でハンドルを引いたらドアを開く
        {
            OpenDoor();
        }

        if (isOpened && !isClosing && IsDoorClosed()) // ドアが開いている状態でドアが閉じたらドアを閉じる
        {
            CloseDoor();
        }
    }

    private bool IsHandlePulled()
    {
        // ハンドルの位置が初期位置からmaxDistance以上離れているかどうか
        return Vector3.Distance(handleObject.position, initialPosition) > maxDistance;
    }

    private void OpenDoor() // ドアを開く
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OnDoorOpened");
    }

    public void OnDoorOpened() // ドアが開いた時の処理
    {
        isOpened = true;
        doorOpenedTime = Time.time;
        isClosing = false;
        PlaySound(openSound);
        DoorOpenAnim();
        doorRigidbody.isKinematic = false;
    }

    private bool IsDoorClosed() // ドアが閉じたかどうか
    {
        // ドアオブジェクトのローカル角度を取得
        Vector3 doorAngle = doorObject.localRotation.eulerAngles;

        // 360度の範囲内に角度を正規化
        doorAngle = NormalizeAngle(doorAngle);

        // 選択された軸に基づいてドアの角度を判定
        float doorAngleSelectedAxis;
        switch (doorAxis)
        {
            case DoorAxis.X:
                doorAngleSelectedAxis = doorAngle.x;
                break;
            case DoorAxis.Y:
                doorAngleSelectedAxis = doorAngle.y;
                break;
            case DoorAxis.Z:
                doorAngleSelectedAxis = doorAngle.z;
                break;
            default:
                doorAngleSelectedAxis = doorAngle.y;
                break;
        }

        // ドアの角度が -closeThreshold 以上かつ closeThreshold 以下かつ、
        // ドアが開いてから一定時間経過したかどうかを判定
        return doorAngleSelectedAxis >= -closeThreshold
            && doorAngleSelectedAxis <= closeThreshold
            && Time.time - doorOpenedTime > doorCloseDelay;
    }

    private Vector3 NormalizeAngle(Vector3 angles)
    {
        angles.x = NormalizeAngle(angles.x);
        angles.y = NormalizeAngle(angles.y);
        angles.z = NormalizeAngle(angles.z);
        return angles;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180)
        {
            angle -= 360;
        }
        return angle;
    }

    private void CloseDoor() // ドアを閉じる
    {
        OnDrop();
        pickup.Drop();
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OnDoorClosed");
    }

    private void ResetDoorFlags() // ドアのフラグをリセットする
    {
        isOpened = false;
        isClosing = false;
    }

    private void PlaySound(AudioClip clip) // サウンドを再生する
    {
        audioSource.PlayOneShot(clip);
    }

    public void OnDoorClosed() // ドアが閉じた時の処理
    {
        isClosing = true;
        doorObject.localRotation = Quaternion.Euler(0, 0, 0);
        PlaySound(closeSound);
        DoorCloseAnim();
        doorRigidbody.isKinematic = true;
    }

    public void DoorOpenAnim() // ドアを開けるアニメーション
    {
        if (animator != null)
        {
            animator.SetBool("EmissiveOn", true);
            animator.SetBool("DoorOpen", true);
        }
    }

    public void DoorCloseAnim() // ドアを閉じるアニメーション
    {
        if (animator != null)
        {
            animator.SetBool("EmissiveOn", false);
            animator.SetBool("DoorOpen", false);
        }
    }
}
