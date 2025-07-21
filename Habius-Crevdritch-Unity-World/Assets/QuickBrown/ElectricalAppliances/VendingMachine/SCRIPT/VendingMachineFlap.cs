using System;
using UdonSharp;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace QuickBrown
{
    public class VendingMachineFlap : UdonSharpBehaviour
    {
        private const float ENABLE_RANGE = 5f;

        private const float FLAP_ROTATION = -90f;

        private Vector3 DefaultRotationEular;

        [SerializeField]
        Transform HingeOrigin;

        [SerializeField]
        float PushMinAngle = 0f;

        [SerializeField]
        float PushMaxAngle = 120f;

        [SerializeField]
        float flipBackSpeed = 2.5f;

        [SerializeField]
        float flapDamping = 0.05f;

        [SerializeField]
        BoxCollider InteractCollider;
        private Bounds interactBounds;

        [SerializeField]
        Transform[] FlapObjects;

        private float CurrentAngle;
        private float CurrentAngleSpeed;

        [SerializeField]
        bool isCountInside = false;

        [SerializeField]
        LayerMask InsideCountlayerMask;
        private int inSideItemCount;

        [SerializeField]
        AudioSource flapAudioSource;

        [SerializeField]
        float flapSoundPitchRange = 0.2f;

        [SerializeField]
        VendingMachine vendingMachine;

        [SerializeField]
        float CallFuncAngle = 60f;

        [SerializeField]
        string CallfuncName;
        private bool isCalledFunc = false;

        [SerializeField]
        Transform DEBUG_HandPosOverride;
        private bool DEBUG_isEnableOverride = false;

        void Start()
        {
            CurrentAngle = 0f;

            DefaultRotationEular = transform.rotation.eulerAngles;
            if (DEBUG_HandPosOverride != null)
                DEBUG_isEnableOverride = true;

            //指定オブジェクトを子に配置
            foreach (var flapObject in FlapObjects)
            {
                flapObject.transform.SetParent(this.transform, true);
            }
        }

        private void FixedUpdate()
        {
            //プレイヤーが近くにいない場合は処理を行わない
            var playerPos = Networking.LocalPlayer.GetPosition();
            if (Vector3.Distance(playerPos, HingeOrigin.position) > ENABLE_RANGE)
            {
                return;
            }

            //コライダー内部のアイテムをカウントして、減っていればパタパタさせる
            if (isCountInside)
            {
                var nextCount = CheckInsideItemCount();
                if (nextCount < inSideItemCount)
                {
                    SetAngleForce(60f);
                    PlaySoundRandomPitch();
                }

                inSideItemCount = nextCount;
            }

            interactBounds = InteractCollider.bounds;
            var isContain = false;
            float pushedAng = 0f;

            if (DEBUG_isEnableOverride)
            {
                if (interactBounds.Contains(DEBUG_HandPosOverride.position))
                {
                    isContain = true;
                    var overrideAng = GetPushRotation(DEBUG_HandPosOverride.position);
                    if (overrideAng > pushedAng)
                    {
                        pushedAng = overrideAng;
                    }
                }
            }

            //コライダーの中に左手があるかどうか
            var leftHand = Networking.LocalPlayer.GetTrackingData(
                VRCPlayerApi.TrackingDataType.LeftHand
            );
            if (interactBounds.Contains(leftHand.position))
            {
                isContain = true;
                var leftAng = GetPushRotation(leftHand.position);
                if (leftAng > pushedAng)
                {
                    pushedAng = leftAng;
                }
            }

            //コライダーの中に右手があるかどうか
            var rightHand = Networking.LocalPlayer.GetTrackingData(
                VRCPlayerApi.TrackingDataType.RightHand
            );
            if (interactBounds.Contains(rightHand.position))
            {
                isContain = true;
                var rightAng = GetPushRotation(rightHand.position);
                if (rightAng > pushedAng)
                {
                    pushedAng = rightAng;
                }
            }

            //止まった状態から動かした場合音を鳴らす
            if (Math.Abs(CurrentAngle) < 0.05f && pushedAng > 0f)
            {
                PlaySoundRandomPitch();
            }

            //一定以上押し込まれた場合、指定の関数を呼び出す
            if (CurrentAngle > CallFuncAngle)
            {
                if (!string.IsNullOrEmpty(CallfuncName))
                {
                    vendingMachine.SendCustomEvent(CallfuncName);

                    //一度関数を呼び出した後は、静止するまで再度呼び出さないようにする
                    isCalledFunc = true;
                }
            }

            //現在の角度＜押される角度の場合（＝より奥に押し込まれている場合）
            if (CurrentAngle < pushedAng && isContain)
            {
                CurrentAngle = pushedAng;
                CurrentAngleSpeed = 0f;
            }
            //それ以外の場合、0度に戻す速度を与える
            else
            {
                if (CurrentAngle > 0f)
                {
                    CurrentAngleSpeed += -flipBackSpeed * Time.fixedDeltaTime;
                }
                else if (CurrentAngle < 0f)
                {
                    CurrentAngleSpeed += flipBackSpeed * Time.fixedDeltaTime;
                }
            }

            //角度を更新
            CurrentAngle += CurrentAngleSpeed;
            CurrentAngle *= (1 - flapDamping);

            //現在の角度と速度が一定以下の際止める
            if (Mathf.Abs(CurrentAngle) < 0.05f && Mathf.Abs(CurrentAngleSpeed) < 0.05f)
            {
                CurrentAngle = 0f;
                CurrentAngleSpeed = 0f;

                //静止したので関数を呼び出し可
                isCalledFunc = false;
            }

            //角度を反映
            transform.rotation = Quaternion.Euler(
                CurrentAngle,
                DefaultRotationEular.y,
                DefaultRotationEular.z
            );
        }

        private void PlaySoundRandomPitch()
        {
            flapAudioSource.pitch =
                1f + UnityEngine.Random.Range(-flapSoundPitchRange, flapSoundPitchRange);
            flapAudioSource.PlayOneShot(flapAudioSource.clip);
        }

        private int CheckInsideItemCount()
        {
            if (InsideCountlayerMask == 0)
                return 0;

            //コライダー内部のアイテムをカウント
            interactBounds = InteractCollider.bounds;
            var interactOrigin = InteractCollider.transform.position + InteractCollider.center;
            return Physics.OverlapBoxNonAlloc(
                interactOrigin,
                interactBounds.extents * 0.9f,
                new Collider[10],
                Quaternion.identity,
                InsideCountlayerMask
            );
        }

        private float GetPushRotation(Vector3 targetPos)
        {
            // ヒンジの原点からターゲットの位置へのベクトルを求める
            Vector3 direction = targetPos - HingeOrigin.position;
            Vector3 normalizedDirection = direction.normalized;

            // 距離に応じて回転角度を計算する
            var rotationAmount =
                Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;
            var rotateFixed = -rotationAmount + FLAP_ROTATION;
            var rotateClamped = Mathf.Clamp(rotateFixed, PushMinAngle, PushMaxAngle);

            return rotateClamped;
        }

        public void SetAngleForce(float angle)
        {
            CurrentAngle = angle;
        }
    }
}
