using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace QuickBrown
{
    public class VendingMachineObjectPool : UdonSharpBehaviour
    {
        public GameObject[] ItemPool;
        
        [UdonSynced(UdonSyncMode.None)] public bool[] ItemPoolUsingSync;

        public bool isEmpty
        {
            get { return _isEmpty(); }
        }

        private bool _isEmpty()
        {
            bool result = true;

            //全てのItemPoolUsingSyncがtrueであれば空
            foreach (var pus in ItemPoolUsingSync)
            {
                if (!pus)
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        public void Awake()
        {
            ItemPoolUsingSync = new bool[ItemPool.Length];
            for (int i = 0; i < ItemPool.Length; i++)
            {
                ItemPoolUsingSync[i] = false;
            }
        }

        public bool SpawnSync()
        {
            return SpawnSync(this.transform.position, this.transform.rotation);
        }

        public bool SpawnSync(Vector3 position, Quaternion rotation)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            
            var spawned = pickItemPool();
            
            //スポーンに成功(=nullでない)
            if (spawned != null)
            {
                Debug.Log($"Spawn Success - {spawned.name}");

                Networking.SetOwner(Networking.LocalPlayer, spawned);
                spawned.transform.SetPositionAndRotation(position, rotation);
                
                //押した本人はここで繁栄
                RefreshActive();

                return true;
            }
            //スポーンに失敗
            else
            {
                Debug.Log("Failed to spawn");
                return false;
            }
        }

        public void RefreshActiveDelay()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(RefreshActive));
        }
        
        public override void OnDeserialization()
        {
            //オーナー以外の場合は、同期状況を再反映
            if (!Networking.IsOwner(this.gameObject))
            {
                RefreshActive();
            }
        }

        private GameObject pickItemPool()
        {
            
            for (int i = 0; i < ItemPool.Length; i++)
            {
                if (!ItemPoolUsingSync[i])
                {
                    Debug.Log($"pickItemPool - index:{i}");
                    
                    ItemPoolUsingSync[i] = true;
                    RequestSerialization();
                    
                    return ItemPool[i];
                }
            }

            return null;
        }
        
        

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            //LateJoinerに同期状況を再反映
            if (player == Networking.LocalPlayer)
            {
                Debug.Log($"OnPlayerJoined: Late-joiner/{player.displayName}");
                SendCustomEventDelayedSeconds(nameof(RefreshActiveDelay), 2f);
            }
        }

        public void RefreshActive()
        {
            Debug.Log($"RefreshActive: poolname:{this.gameObject.name}");

            for (int i = 0; i < ItemPool.Length; i++)
            {
                Debug.Log($"RefreshActive: index:{i}/{ItemPoolUsingSync[i]}");
                ItemPool[i].SetActive(ItemPoolUsingSync[i]);
            }
        }
    }
}