using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public partial class ProtagController : MonoBehaviour
{
    private float jumpTimer = 0f;

    private Vector2 jumpVelocity;
    private bool doVariableJump = true;

    // Jump State
    public void EnterJumpState()
    {
        DoJump();
        jumpTimer = ControllerConfig.JumpDuration;
        inputProvider.OnJumpReleased += ExitJumpEarly;
        inputProvider.OnSpecialAbilityPressed += TryGrapplingSwitch;

        AudioManager.Instance.PlaySFX(SFX.Jump, transform.position);
        animator.Play("Jump");
    }

    public void ExitJumpState()
    {
        inputProvider.OnJumpReleased -= ExitJumpEarly;
        inputProvider.OnSpecialAbilityPressed -= TryGrapplingSwitch;
    }

    private void ExitJumpEarly()
    {
        if (doVariableJump)
        {
            charController.Velocity = new Vector2(
                charController.Velocity.x,
                charController.Velocity.y / 2.5f);
        }
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
            var slideInput = inputProvider.VerticalAxis < 0f;
            var vel = Rb.velocity;
            var enoughSpeed = vel.magnitude >= ControllerConfig.SlideEnterMinVelocity;
            var normal = charController.CurrentStateContext.groundNormal;

            // Flat surface counts as a slope
            var goingDownSlope = normal.x * vel.x >= 0f || Mathf.Abs(normal.x) < 0.01f;
            if (slideInput && 
                ControllerConfig.CanSlide && 
                enoughSpeed && 
                goingDownSlope && 
                slideCooldownTimer <= 0f)
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
        doVariableJump = false;
        jumpVelocity = new Vector2(0f, ControllerConfig.JumpVelocity);
    }
}