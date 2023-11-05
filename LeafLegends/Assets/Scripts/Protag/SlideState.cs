using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ProtagController : MonoBehaviour
{
    private bool canExitCrouch;

    // Crouch State
    public void EnterCrouchState()
    {
        inputProvider.OnJumpPressed += TryJumpFromCrouch;

        charController.OffsetCapsuleHeight(-ControllerConfig.SlideHeightReduction);
        animator.Play("Sliding");
    }

    public void ExitCrouchState()
    {
        inputProvider.OnJumpPressed -= TryJumpFromCrouch;

        charController.OffsetCapsuleHeight(ControllerConfig.SlideHeightReduction);
    }

    public void UpdateCrouchState()
    {
        canExitCrouch = CanExitCrouch();

        if (!stableOnGround && canExitCrouch)
        {
            SwitchStates(ProtagStates.Airborne);
        }

        // Let go of crouch
        if (inputProvider.VerticalAxis >= 0f && canExitCrouch)
        {
            if (TryGroundSwitch())
            {
                return;
            }
        }
    }

    private void TryJumpFromCrouch()
    {
        if (canExitCrouch)
        {
            TrySwitchToJumpState();
        }
    }

    public void FixedUpdateCrouchState()
    {
        var groundNormal = charController.CurrentStateContext.groundNormal;
        // Calculate new velocity
        var groundAngle = Vector2.SignedAngle(Vector2.up, groundNormal);

        // Transform velocity to "ground" tangent space
        Vector2 relativeVelocity = Quaternion.Euler(0, 0, -groundAngle) * charController.Velocity;

        // Maintain a minimum sliding velocity if under an obstacle
        if (!canExitCrouch &&
            Mathf.Abs(relativeVelocity.x) < ControllerConfig.SlideMinVelocity)
        {
            relativeVelocity.x = Mathf.Sign(relativeVelocity.x) * ControllerConfig.SlideMinVelocity;
        }

        // Gravity
        relativeVelocity += (Vector2)(Quaternion.Euler(0, 0, -groundAngle) * Vector2.down) * (Time.fixedDeltaTime * ControllerConfig.SlideGravityAccel);

        charController.Velocity = Quaternion.Euler(0, 0, groundAngle) * relativeVelocity;

        // Sliding should snap to ground
        currentMoveInput.horizontalInput = Mathf.Sign(relativeVelocity.x);
        charController.SnapToGround(ControllerConfig, currentMoveInput);
    }

    private bool CanExitCrouch()
    {
        var colliders = charController.OverlapCapsule(
            new Vector2(0, ControllerConfig.SlideHeightReduction / 2),
            0.05f,
            -ControllerConfig.SlideHeightReduction + 0.05f,
            ControllerConfig.GroundLayerMask
        );

        if (colliders.Length > 0)
        {
            return false;
        }

        return true;
    }
}