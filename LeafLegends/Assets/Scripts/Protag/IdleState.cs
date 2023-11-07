using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ProtagController : MonoBehaviour
{
    private bool stableOnGround => charController.CurrentStateContext.stableOnGround;

    private float idleTime;

    // Idle State
    public void EnterIdleState()
    {
        ResetJumpVelocity();
        inputProvider.OnJumpPressed += TrySwitchToJumpState;
        inputProvider.OnSpecialAbilityPressed += TryGrapplingSwitch;
        idleTime = 0f;
    }

    public void ExitIdleState()
    {
        inputProvider.OnJumpPressed -= TrySwitchToJumpState;
        inputProvider.OnSpecialAbilityPressed -= TryGrapplingSwitch;
    }

    public void UpdateIdleState()
    {
        idleTime += Time.deltaTime;
        if (idleTime > 0.1f)
        {
            animator.Play("Idle");
        }
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