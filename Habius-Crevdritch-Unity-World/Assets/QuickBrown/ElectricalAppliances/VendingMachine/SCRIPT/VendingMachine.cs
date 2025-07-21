using System;
using TMPro;
using UCS;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;

namespace QuickBrown
{
    public class VendingMachine : UdonSharpBehaviour
    {
        [Header("UdonChipsを設定してください（Questでは必須）\n/Set up UdonChips (required by Quest)")]
        public UdonChips udonChips;

        [Header("-- 商品設定 --")]
        [Header("商品ボタンの参照")]
        public VendingMachineShopButton[] ItemSelectButtons;

        [Header("商品ボタンとプールのインデックスの対応表")]
        public int[] ItemKindIndex;

        [Header("商品価格")]
        public float[] ItemPrices;

        //飲み物オブジェクトのプール
        public VendingMachineObjectPool[] ItemObjectPools;

        [Header("-- 位置設定 --")]
        //選択ランプのリスト
        [Tooltip("選択ランプの参照")]
        public Image[] SelectionLamps;

        //売り切れランプのリスト
        [Tooltip("売り切れランプの参照")]
        public Image[] SoldOutLamps;

        [SerializeField]
        Transform ItemSpawnPoint;

        //お釣りがあるときに表示するオブジェクト(強調表示など)
        [SerializeField]
        GameObject ChangeEnableObject;

        //投入金額のテキスト
        [SerializeField]
        TextMeshPro DepositCoinText;

        [SerializeField]
        TextMeshPro HaveCoinText;

        //待機アニメーション
        [SerializeField]
        Animator WaitingAnimator;

        [Header("-- サウンド設定 --")]
        //購入時のサウンド
        [SerializeField]
        AudioSource DrinkEjectSound;

        [SerializeField]
        float DrinkEjectSoundPitchRange = 0.2f;

        //コイン排出時のサウンド
        [SerializeField]
        AudioSource CoinEjectSound;

        [SerializeField]
        float CoinEjectSoundPitchRange = 0.2f;

        //コイン投入時のサウンド
        [SerializeField]
        AudioSource CoinInsertSound;

        [SerializeField]
        float CoinInsertSoundPitchRange = 0.2f;

        //アイテム選択時のサウンド
        [SerializeField]
        AudioSource SelectSound;

        [SerializeField]
        AudioSource CoinPickSound;

        [Header("-- コイン勝手に貯まる設定 --")]
        private float nextCoinStockTime;

        [SerializeField]
        float AutoCoinStockDurationFixed = 10f;

        [SerializeField]
        float AutoCoinStockDurationRandom = 10f;

        [SerializeField]
        float AutoCoinStockAmount = 100f;

        //同期変数
        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(SelectionItemIndexSync))]
        private int _selectionItemIndex = -1;

        private int SelectionItemIndexSync
        {
            get => _selectionItemIndex;
            set
            {
                _selectionItemIndex = value;
                SelectionItemIndexChanged();
            }
        }

        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(DepositCoinAmountSync))]
        public float _depositCoinAmountSync = 0;

        private float DepositCoinAmountSync
        {
            get => _depositCoinAmountSync;
            set
            {
                _depositCoinAmountSync = value;
                DepositCoinAmountSyncChanged();
            }
        }

        [UdonSynced(UdonSyncMode.None)]
        private int TouchPlayerIdSync = 0;

        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(ChangeStockCoinSync))]
        private float _changeStockCoinSync = 0;
        private float ChangeStockCoinSync
        {
            get => _changeStockCoinSync;
            set
            {
                _changeStockCoinSync = value;
                ChangeStockCoinSyncChanged();
            }
        }

        [UdonSynced(UdonSyncMode.None)]
        private bool hasChange;

        //モード
        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(CurrentModeSync))]
        private int _currentMode = 0;

        private int CurrentModeSync
        {
            get => _currentMode;
            set
            {
                _currentMode = value;
                ModeRooter(value);
            }
        }

        //モード用定数
        private const int MODE_SLEEP = 0;
        private const int MODE_ACTIVE = 100;
        private const int MODE_SELECTION = 300;
        private const int MODE_SELECTION_WALLET = 301;
        private const int MODE_PURCHASE = 400;
        private const int MODE_EJECT = 500;
        private const int MODE_RETURN = 1000;

        private int PurchaseMode = 0;
        private const int MODE_PURCHASE_NONE = 0;
        private const int PURCHASE_COIN = 1;
        private const int PURCHASE_WALLET = 2;

        private float deactiveTime = 0;
        private const float ACTIVE_DURATIOM = 60f;

        void Start()
        {
            if (udonChips == null)
            {
                GameObject udonChipsObject = GameObject.Find("UdonChips");
                if (udonChipsObject != null)
                {
                    udonChips = udonChipsObject.GetComponent<UdonChips>();
                    Debug.LogError("UdonChipsが見つかりませんでした。手動設定してください。");
                }
            }

            for (int i = 0; i < ItemSelectButtons.Length; i++)
            {
                ItemSelectButtons[i].SetItemIndex(i);
            }

            //アクティブ切り替え
            DepositCoinText.gameObject.SetActive(true);
            HaveCoinText.gameObject.SetActive(true);

            //初期設定
            RefreshSelectionObjects();

            StartVendingMachine();
        }

        public void Update()
        {
            //オーナーのみ処理
            if (Networking.IsOwner(this.gameObject))
            {
                //コイン勝手に貯まる
                if (AutoCoinStockAmount > 0 && Time.time > nextCoinStockTime)
                {
                    ChangeStockCoinSync += AutoCoinStockAmount;
                    Debug.Log($"AutoCoinStockAmount / {ChangeStockCoinSync}");

                    RequestSerialization();
                    SetNextCoinTime();
                }

                //選択モードが一定時間放置で戻す
                if (CurrentModeSync == MODE_SELECTION || CurrentModeSync == MODE_SELECTION_WALLET)
                {
                    if (Time.time > deactiveTime)
                    {
                        ReturnCoin();
                    }
                }
            }
        }

        //モード遷移先に応じて同期が必要な処理を行う
        private void ModeRooter(int dstMode)
        {
            Debug.Log($"ModeRooter: {dstMode}");
            switch (dstMode)
            {
                //アイテム選択(コイン投入)
                case MODE_SELECTION:
                    DepositCoinSync();
                    break;
                //アイテム選択(おサイフ)
                case MODE_SELECTION_WALLET:
                    TouchWalletSync();
                    break;
                //アイテム購入
                case MODE_PURCHASE:
                    //SelectItemSync();
                    break;
                //アイテム排出
                case MODE_EJECT:
                    EjectItemSync();
                    break;
                //コイン返却
                case MODE_RETURN:
                    ReturnCoinSync();
                    break;
            }
        }

        public void StartVendingMachine()
        {
            CurrentModeSync = MODE_ACTIVE;
            PurchaseMode = MODE_PURCHASE_NONE;

            WaitingAnimator.enabled = true;

            DepositCoinText.text = "";
            HaveCoinText.text = "";

            DepositCoinAmountSync = 0;
            TouchPlayerIdSync = 0;

            //次回コイン貯まるタイムを設定
            SetNextCoinTime();
        }

        private void SetNextCoinTime()
        {
            if (AutoCoinStockDurationFixed <= 0)
                return;
            if (AutoCoinStockAmount <= 0)
                return;

            nextCoinStockTime =
                Time.time
                + AutoCoinStockDurationFixed
                + UnityEngine.Random.Range(0, AutoCoinStockDurationRandom);
        }

        //おサイフタッチ
        public bool TouchWallet()
        {
            if (CurrentModeSync != MODE_ACTIVE)
                return false;

            if (udonChips == null)
                return false;

            if (udonChips.money <= 0)
                return false;

            //オーナー取得
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

            CurrentModeSync = MODE_SELECTION_WALLET;
            TouchPlayerIdSync = Networking.LocalPlayer.playerId;

            //預り金は所持UdonChipsの金額
            DepositCoinAmountSync += udonChips.money;
            PurchaseMode = PURCHASE_WALLET;

            //同期要求
            RequestSerialization();

            return true;
        }

        public void TouchWalletSync()
        {
            deactiveTime = Time.time + ACTIVE_DURATIOM;
            PurchaseMode = PURCHASE_WALLET;

            WaitingAnimator.enabled = false;

            //選択音を鳴らす
            if (SelectSound != null)
                PlaySoundRandomPitch(SelectSound);
        }

        //コイン投入
        public bool DepositCoin(float coin)
        {
            if (PurchaseMode == PURCHASE_WALLET)
                return false;

            //オーナー取得
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

            CurrentModeSync = MODE_SELECTION;

            //預かり金に投入コイン分加算
            DepositCoinAmountSync += coin;
            PurchaseMode = PURCHASE_COIN;

            Debug.Log($"DepositCoin: add:{coin} - total:{DepositCoinAmountSync}");

            //同期要求
            RequestSerialization();

            return true;
        }

        public void DepositCoinSync()
        {
            //次回アクティブ解除時間までのタイマーを設定
            deactiveTime = Time.time + ACTIVE_DURATIOM;
            PurchaseMode = PURCHASE_COIN;

            WaitingAnimator.enabled = false;

            //コイン音を鳴らす
            if (CoinInsertSound != null)
                PlaySoundRandomPitch(CoinInsertSound, CoinInsertSoundPitchRange);
        }

        public void DepositCoinAmountSyncChanged()
        {
            Debug.Log($"DepositCoinAmountSyncChanged: total:{DepositCoinAmountSync}");

            if (CurrentModeSync == MODE_SELECTION_WALLET)
            {
                HaveCoinText.text = DepositCoinAmountSync.ToString();
            }
            else if (CurrentModeSync == MODE_SELECTION)
            {
                DepositCoinText.text = DepositCoinAmountSync.ToString();
            }

            RefreshSelectionObjects();
        }

        //投入金額に応じてアイテム選択ボタンなどを切り替え
        public void RefreshSelectionObjects()
        {
            for (int i = 0; i < ItemSelectButtons.Length; i++)
            {
                if (!ItemSelectButtons[i])
                    continue;

                var kindIndex = ItemKindIndex[i];

                bool isSoldOut = ItemObjectPools[kindIndex].isEmpty;

                bool canSelect = true;

                //商品選択モード以外は選択不可
                if (CurrentModeSync != MODE_SELECTION && CurrentModeSync != MODE_SELECTION_WALLET)
                    canSelect = false;
                //アイテムが売り切れの場合は選択不可
                else if (isSoldOut)
                    canSelect = false;
                //投入コインが足りない場合は選択不可
                else if (DepositCoinAmountSync < ItemPrices[i])
                    canSelect = false;

                //選択ボタン&ランプ切り替え
                if (canSelect)
                {
                    ItemSelectButtons[i].gameObject.SetActive(true);
                    SelectionLamps[i].gameObject.SetActive(true);
                }
                else
                {
                    ItemSelectButtons[i].gameObject.SetActive(false);
                    SelectionLamps[i].gameObject.SetActive(false);
                }

                //売り切れランプ切り替え
                if (isSoldOut)
                {
                    SoldOutLamps[i].gameObject.SetActive(true);
                }
                else
                {
                    SoldOutLamps[i].gameObject.SetActive(false);
                }
            }
        }

        //アイテム決定
        public void SelectItem(int itemIndex)
        {
            if (CurrentModeSync != MODE_SELECTION && CurrentModeSync != MODE_SELECTION_WALLET)
                return;

            //コインが足りない場合は何もしない
            if (DepositCoinAmountSync < ItemPrices[itemIndex])
            {
                return;
            }

            //オーナー取得
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

            //コインを減らす
            DepositCoinAmountSync -= ItemPrices[itemIndex];

            //選択したアイテムを記録
            SelectionItemIndexSync = itemIndex;

            CurrentModeSync = MODE_PURCHASE;

            //同期要求
            RequestSerialization();

            //遅延してアイテム排出
            SendCustomEventDelayedSeconds(nameof(EjectItem), 1.0f);
        }

        private void SelectionItemIndexChanged()
        {
            Debug.Log($"SelectionItemIndexChanged - itemIndex: {SelectionItemIndexSync}");

            //選択音を再生
            if (SelectSound != null)
                PlaySoundRandomPitch(SelectSound);

            //選択したアイテムのランプを点灯
            SetSelectLamp();
        }

        public void SetSelectLamp()
        {
            int selectedIndex = SelectionItemIndexSync;

            //選択したアイテムのランプを点灯
            for (int i = 0; i < ItemSelectButtons.Length; i++)
            {
                if (!ItemSelectButtons[i])
                    continue;
                if (!SelectionLamps[i])
                    continue;

                //選択ボタンを無効
                ItemSelectButtons[i].gameObject.SetActive(false);

                //ランプ切り替え
                if (i == selectedIndex)
                {
                    SelectionLamps[i].gameObject.SetActive(true);
                }
                else
                {
                    SelectionLamps[i].gameObject.SetActive(false);
                }
            }
        }

        //アイテム排出
        public void EjectItem()
        {
            //オーナー取得
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

            //アイテムの排出
            int selectedItemKindIndex = ItemKindIndex[SelectionItemIndexSync];
            ItemObjectPools[selectedItemKindIndex]
                .SpawnSync(ItemSpawnPoint.position, ItemSpawnPoint.rotation);

            if (PurchaseMode == PURCHASE_COIN)
            {
                //お釣りの排出
                ChangeStockCoinSync += DepositCoinAmountSync;
            }
            else if (PurchaseMode == PURCHASE_WALLET)
            {
                //SYNCの方で処理
            }

            Debug.Log($"EjectItem: deposit:{DepositCoinAmountSync}");

            CurrentModeSync = MODE_EJECT;
            RequestSerialization();
        }

        public void EjectItemSync()
        {
            //排出音を再生
            if (DrinkEjectSound != null)
                PlaySoundRandomPitch(DrinkEjectSound, DrinkEjectSoundPitchRange);

            //釣り銭音は少し遅れて再生
            SendCustomEventDelayedSeconds(nameof(EjectCoinDelayed), 1.0f);

            //udonchipsの金額を更新
            //おサイフタッチしたプレイヤーの金額を減らす
            if (TouchPlayerIdSync == Networking.LocalPlayer.playerId)
            {
                var puchaseItemPrice = ItemPrices[SelectionItemIndexSync];
                udonChips.money -= puchaseItemPrice;
                if (udonChips.money < 0)
                    udonChips.money = 0;
            }

            Debug.Log(
                $"EjectItemSync: deposit:{DepositCoinAmountSync} / change:{ChangeStockCoinSync}"
            );

            RefreshSelectionObjects();
        }

        public void EjectCoinDelayed()
        {
            //お釣り音を再生
            if (PurchaseMode == PURCHASE_COIN && DepositCoinAmountSync > 0)
            {
                //排出音を再生
                if (CoinEjectSound != null)
                    PlaySoundRandomPitch(CoinEjectSound, CoinEjectSoundPitchRange);
            }

            //少し遅れて待機モードに戻す
            SendCustomEventDelayedSeconds(nameof(StartVendingMachine), 1.0f);
        }

        //コイン返却
        public void ReturnCoin()
        {
            //選択中のみ操作可
            if (CurrentModeSync != MODE_SELECTION && CurrentModeSync != MODE_SELECTION_WALLET)
                return;

            //オーナー取得
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

            Debug.Log($"ReturnCoin: PurchaseMode:{PurchaseMode}");

            //お釣りに預り金を加算
            if (PurchaseMode == PURCHASE_COIN)
            {
                ChangeStockCoinSync += DepositCoinAmountSync;
            }

            CurrentModeSync = MODE_RETURN;

            RequestSerialization();
        }

        public void ReturnCoinSync()
        {
            //お釣り音を再生
            if (PurchaseMode == PURCHASE_COIN && DepositCoinAmountSync > 0)
            {
                //排出音を再生
                if (CoinEjectSound != null)
                    PlaySoundRandomPitch(CoinEjectSound, CoinEjectSoundPitchRange);
            }

            Debug.Log(
                $"ReturnCoinSync: deposit:{DepositCoinAmountSync} / change:{ChangeStockCoinSync}"
            );

            RefreshSelectionObjects();

            //standbyモードに戻す
            SendCustomEventDelayedSeconds(nameof(StartVendingMachine), 1.0f);
        }

        //おつりコインを取る
        public void PickChargeCoin()
        {
            if (udonChips == null)
                return;

            if (ChangeStockCoinSync <= 0)
                return;

            //オーナー取得
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

            //サウンドを再生
            if (CoinPickSound != null)
                PlaySoundRandomPitch(CoinPickSound);

            //取得者の所持金におつりストック加算,ストックを0に
            udonChips.money += ChangeStockCoinSync;
            ChangeStockCoinSync = 0;

            RequestSerialization();
        }

        private void ChangeStockCoinSyncChanged()
        {
            //おつり表示を切り替え
            ChangeEnableObject.SetActive(ChangeStockCoinSync > 0);
        }

        private void PlaySoundRandomPitch(AudioSource audioSource, float pitchRange = 0f)
        {
            if (pitchRange > 0f)
                audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchRange, pitchRange);
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}
