using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ProtagController : MonoBehaviour
{
    private bool stableOnGround => charController.CurrentStateContext.stableOnGround;

    // Idle State
    public void EnterIdleState()
    {
        ResetJumpVelocity();
        inputProvider.OnJumpPressed += TrySwitchToJumpState;
    }

    public void ExitIdleState()
    {
        inputProvider.OnJumpPressed -= TrySwitchToJumpState;
    }

    public void UpdateIdleState()
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

    public void FixedUpdateIdleState()
    {
        DefaultGroundedMovement();
    }
}