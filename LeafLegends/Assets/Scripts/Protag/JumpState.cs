using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ProtagController : MonoBehaviour
{
    private float jumpTimer = 0f;

    private Vector2 jumpVelocity;

    // Jump State
    public void EnterJumpState()
    {
        DoJump();
        jumpTimer = ControllerConfig.JumpDuration;
        inputProvider.OnJumpReleased += ExitJumpEarly;
    }

    public void ExitJumpState()
    {
        inputProvider.OnJumpReleased -= ExitJumpEarly;
    }

    private void ExitJumpEarly()
    {
        charController.Velocity = new Vector2(
            charController.Velocity.x,
            charController.Velocity.y / 2.5f);
        SwitchStates(ProtagStates.Airborne);
    }

    public void UpdateJumpState()
    {
        jumpTimer -= Time.deltaTime;
        // Exit jump if we bonk our head
        if (jumpTimer < 0f || charController.Velocity.y <= 0)
        {
            SwitchStates(ProtagStates.Airborne);
            return;
        }

        if (TryGroundSwitch())
        {
            return;
        }

        if (jumpTimer < ControllerConfig.JumpDuration - 0.2f && TryWallSlideSwitch())
        {
            return;
        }
    }

    public void FixedUpdateJumpState()
    {
        charController.HorizontalMovement(ControllerConfig, currentMoveInput);
        charController.AirbornePhysics(ControllerConfig);
    }

    private void DoJump()
    {
        charController.ForceAirborne(0.2f);
        var vel = charController.Velocity;
        vel.y = ControllerConfig.JumpVelocity;

        if (jumpVelocity.x != 0)
        {
            vel.x = jumpVelocity.x;
        }

        charController.Velocity = vel;
    }

    public void TrySwitchToJumpState()
    {
        if (charController.CoyoteTime <= ControllerConfig.CoyoteTime)
        {
            SwitchStates(ProtagStates.JumpState);
        }
    }

    public bool TryGroundSwitch()
    {
        if (!stableOnGround)
        {
            return false;
        }

        if (currentMoveInput.horizontalInput != 0)
        {
            if (inputProvider.VerticalAxis < 0f && ControllerConfig.CanSlide)
            {
                return SwitchStates(ProtagStates.Sliding);
            }
            else
            {
                return SwitchStates(ProtagStates.Running);
            }
        }
        else
        {
            return SwitchStates(ProtagStates.Idle);
        }

        return false;
    }

    private void ResetJumpVelocity()
    {
        jumpVelocity = new Vector2(0f, ControllerConfig.JumpVelocity);
    }
}