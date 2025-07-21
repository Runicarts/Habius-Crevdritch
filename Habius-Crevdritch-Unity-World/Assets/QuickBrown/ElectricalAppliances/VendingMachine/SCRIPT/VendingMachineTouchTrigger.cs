
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace QuickBrown
{
    public class VendingMachineTouchTrigger : UdonSharpBehaviour
    {
        [SerializeField] VendingMachine vendingMachine;

        [SerializeField] string[] ObjectNames;
        
        [SerializeField] string CallFunc;
        
        public void OnCollisionEnter(Collision other)
        {
            Debug.Log($"VendingMachineTouchTrigger / OnCollisionEnter: name: {other.gameObject.name}");
            foreach (var name in ObjectNames)
            {
                if (other.gameObject.name == name)
                {
                    vendingMachine.SendCustomEvent(CallFunc);
                }
            }

        }
        public void OnTriggerEnter(Collider other)
        {
            Debug.Log($"VendingMachineTouchTrigger / onTriggerEnter: name: {other.gameObject.name}");
            foreach (var name in ObjectNames)
            {
                if (other.gameObject.name == name)
                {
                    vendingMachine.SendCustomEvent(CallFunc);
                }
            }

        }
    }
}
