using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ProtagController : MonoBehaviour
{
    // Running State
    public void EnterRunningState()
    {
        ResetJumpVelocity();
        inputProvider.OnJumpPressed += TrySwitchToJumpState;
    }

    public void ExitRunningState()
    {
        inputProvider.OnJumpPressed -= TrySwitchToJumpState;
    }

    public void UpdateRunningState()
    {
        if (!stableOnGround)
        {
            SwitchStates(ProtagStates.Airborne);
        }

        if (TryGroundSwitch())
        {
            return;
        }
    }

    public void FixedUpdateRunningState()
    {
        DefaultGroundedMovement();
    }

    private void DefaultGroundedMovement()
    {
        charController.HorizontalMovement(ControllerConfig, currentMoveInput);
        charController.SnapToGround(ControllerConfig, currentMoveInput);
    }
}