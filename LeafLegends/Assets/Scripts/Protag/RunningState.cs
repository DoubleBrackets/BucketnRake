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
        inputProvider.OnSpecialAbilityPressed += TryGrapplingSwitch;
    }

    public void ExitRunningState()
    {
        inputProvider.OnJumpPressed -= TrySwitchToJumpState;
        inputProvider.OnSpecialAbilityPressed -= TryGrapplingSwitch;
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

        if (Rb.velocity.magnitude > 1f && Rb.velocity.x * currentMoveInput.horizontalInput > 0)
        {
            animator.Play("Run");
        }
        else
        {
            animator.Play("Idle");
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