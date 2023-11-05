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
        inputProvider.OnSpecialAbilityPressed += TryGrapplingSwitch;
    }

    public void ExitAirborneState()
    {
        inputProvider.OnJumpPressed -= TrySwitchToJumpState;
        inputProvider.OnSpecialAbilityPressed -= TryGrapplingSwitch;
    }

    public void UpdateAirborneState()
    {
        if (TryGroundSwitch())
        {
            AudioManager.Instance.PlaySFX(SFX.Land, transform.position);
            return;
        }

        if (TryWallSlideSwitch())
        {
            return;
        }

        if (Rb.velocity.y > 0)
        {
            animator.Play("Jump");
        }
        else
        {
            animator.Play("Airborne");
        }
    }

    public void FixedUpdateAirborneState()
    {
        charController.AirbornePhysics(ControllerConfig);
        charController.HorizontalMovement(ControllerConfig, currentMoveInput);
    }
}