
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WelcomeElevator_Button : UdonSharpBehaviour
{
    [SerializeField] WelcomeElevator welcomeElevator;

    public override void Interact()
    {
        welcomeElevator.Open();
        welcomeElevator.doorIsOpen = true;
    }
}
