using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ProtagController : MonoBehaviour
{
    // Airborne State
    public void EnterAirborneState()
    {
        // in case of coyote time
        inputProvider.OnJumpPressed += TrySwitchToJumpState;
    }

    public void ExitAirborneState()
    {
        inputProvider.OnJumpPressed -= TrySwitchToJumpState;
    }

    public void UpdateAirborneState()
    {
        if (TryGroundSwitch())
        {
            return;
        }

        if (TryWallSlideSwitch())
        {
            return;
        }
    }

    public void FixedUpdateAirborneState()
    {
        charController.AirbornePhysics(ControllerConfig);
        charController.HorizontalMovement(ControllerConfig, currentMoveInput);
    }
}