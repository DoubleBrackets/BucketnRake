using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ProtagController : MonoBehaviour
{
    private float wallSlideDirection;

    // WallSlide State
    public void EnterWallSlideState()
    {
        inputProvider.OnJumpPressed += TrySwitchToJumpState;
        PrepWallJump();

        AudioManager.Instance.PlaySFX(SFX.WallSlideStartup, transform.position);
        animator.Play("Wallslide");
    }

    public void ExitWallSlideState()
    {
        inputProvider.OnJumpPressed -= TrySwitchToJumpState;
    }

    public void UpdateWallSlideState()
    {
        if (TryGroundSwitch())
        {
            return;
        }

        if (!CheckWallSliding())
        {
            SwitchStates(ProtagStates.Airborne);
        }

        charController.CoyoteTime = 0f;
    }

    public void FixedUpdateWallSlideState()
    {
        charController.AirbornePhysics(ControllerConfig);
        if (currentMoveInput.horizontalInput != 0)
        {
            charController.Velocity = new Vector2(
                currentMoveInput.horizontalInput * 2f,
                Mathf.Max(charController.Velocity.y, ControllerConfig.WallSlideVel));
        }
    }

    private void PrepWallJump()
    {
        jumpVelocity.x = -currentMoveInput.horizontalInput * ControllerConfig.WallJumpVel;
    }

    private bool TryWallSlideSwitch()
    {
        var stable = charController.CurrentStateContext.stableOnGround;
        if (ControllerConfig.CanWallSlide && CheckWallSliding() && !stable)
        {
            return SwitchStates(ProtagStates.WallSlide);
        }

        return false;
    }

    private bool CheckWallSliding()
    {
        if (currentMoveInput.horizontalInput == 0 || charController.CurrentStateContext.touchingGround)
        {
            return false;
        }


        var hit = charController.CastCapsule(
            0.1f,
            ControllerConfig.WallSlideCastSizeYReduction,
            Vector2.right * currentMoveInput.horizontalInput,
            0.2f,
            ControllerConfig.WallSlideMask);

        // Is this actually a wall
        if (hit.collider)
        {
            bool isWall = Vector2.Angle(Vector2.up, hit.normal) > 85;
            return isWall;
        }

        return false;
    }
}