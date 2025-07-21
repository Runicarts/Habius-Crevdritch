
using UCS;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace QuickBrown
{
    public class VendingMachineTrigger : UdonSharpBehaviour
    {
        [SerializeField] VendingMachine vendingMachine;
        [SerializeField] AudioSource audioSource;
        [SerializeField] Animator animator;
        [SerializeField] string animatorTriggerName;
        [SerializeField] string CallFunc;
        
        public override void Interact()
        {
            vendingMachine.SendCustomEvent(CallFunc);
            if(animator != null)animator.SetTrigger(animatorTriggerName);
            if(audioSource != null)audioSource.Play();
        }
    }    
}

