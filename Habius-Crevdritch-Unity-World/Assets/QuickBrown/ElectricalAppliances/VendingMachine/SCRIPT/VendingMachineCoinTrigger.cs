using UCS;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace QuickBrown
{
    public class VendingMachineCoinTrigger : UdonSharpBehaviour
    {
        [SerializeField] VendingMachine vendingMachine;
        [SerializeField] float coinValue = 100f;

        public override void Interact()
        {
            //UdonChipの参照がある場合、UdonChipの残金を確認して投入
            if (vendingMachine.udonChips != null)
            {
                if (vendingMachine.udonChips.money <= 0)
                {
                    return;
                }
                else if (vendingMachine.udonChips.money < coinValue)
                {
                    var insertValue = vendingMachine.udonChips.money;

                    if (vendingMachine.DepositCoin(insertValue))
                    {
                        vendingMachine.udonChips.money = 0;
                    }
                }
                else
                {
                    if (vendingMachine.DepositCoin(coinValue))
                    {
                        vendingMachine.udonChips.money -= coinValue;
                    }
                }
            }
            else
            {
                //そのまま投入
                vendingMachine.DepositCoin(coinValue);
            }
        }
    }
}