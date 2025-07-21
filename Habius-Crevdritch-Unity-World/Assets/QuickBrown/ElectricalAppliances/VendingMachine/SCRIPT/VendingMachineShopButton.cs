
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace QuickBrown
{
    public class VendingMachineShopButton : UdonSharpBehaviour
    {
        public VendingMachine vendingMachine;
        private int _ItemIndex;
        
        public void SetItemIndex(int index)
        {
            _ItemIndex = index;
        }
        
        public override void Interact()
        {
            vendingMachine.SelectItem(_ItemIndex);
        }
    }    
} 

