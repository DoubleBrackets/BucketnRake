using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public partial class ProtagController : MonoBehaviour
{
    private bool isGrappling = false;
    private GrapplePoint pointBeingGrappled;
    
    private float grapplePullDuration;

    private Vector2 grappleVector;
    private Vector2 grappleStartPos;

    private Vector2 preGrappleVel;

    private float grappleCooldownTimer;


    // Grappling State
    public void EnterGrapplingState()
    {
        pointBeingGrappled = targetedGrapplePoint;
        FollowCameraScript.Instance.AdditionalTargets.Add(pointBeingGrappled.transform);
        isGrappling = true;
        preGrappleVel = Rb.velocity;
        grappleCooldownTimer = ControllerConfig.GrappleCooldown;

        // Calculate the vector of pulling
        grappleVector = targetedGrapplePoint.transform.position - transform.position;
        var dist = Mathf.Max(0.01f, grappleVector.magnitude - ControllerConfig.GrappleEndOffset);
        grappleVector = grappleVector.normalized * dist;

        // Calculate the duration of the pull
        grapplePullDuration = grappleVector.magnitude / ControllerConfig.GrapplePullSpeed;


        // Set up the bodies based on whose pulling who
        charController.Rb.bodyType = RigidbodyType2D.Static;
        if (pointBeingGrappled.isPullGrapple)
        {
            grappleStartPos = pointBeingGrappled.rb.transform.position;
            grappleVector = -grappleVector;
            pointBeingGrappled.rb.bodyType = RigidbodyType2D.Static;
            pointBeingGrappled.controller.ForceAirborne(0.2f);
        }
        else
        {
            grappleStartPos = transform.position;
        }

        charController.ForceAirborne(0.2f);

        animator.Play("Grapple");

        DoGrapple();
    }

    public void ExitGrapplingState()
    {
        isGrappling = false;
        FollowCameraScript.Instance.AdditionalTargets.Remove(pointBeingGrappled.transform);
        Rb.bodyType = RigidbodyType2D.Dynamic;
        if (pointBeingGrappled.isPullGrapple)
        {
            pointBeingGrappled.rb.bodyType = RigidbodyType2D.Dynamic;
            pointBeingGrappled.rb.velocity = grappleVector.normalized * ControllerConfig.GrappleExitSpeed;

            // Return to normal speed
            Rb.velocity = preGrappleVel;
        }
        else
        {
            charController.Velocity = grappleVector.normalized * ControllerConfig.GrappleExitSpeed;
        }
    }

    public void UpdateGrapplingState()
    {
    }

    private async UniTaskVoid DoGrapple()
    {
        AudioManager.Instance.PlaySFX(SFX.GrappleThrow, transform.position);
        var totalDist = grappleVector.magnitude + ControllerConfig.GrappleEndOffset;
        await grappleVisuals.WindupAnimation(pointBeingGrappled, totalDist / ControllerConfig.GrappleProjectileSpeed);

        grappleVisuals.LinkPoint(pointBeingGrappled);
        // Pulling stage
        AudioManager.Instance.PlaySFX(SFX.GrapplePull, transform.position);
        var grapplePullTimer = 0f;
        while (grapplePullTimer < grapplePullDuration)
        {
            var t = Mathf.Min(1, grapplePullTimer / grapplePullDuration);

            if (pointBeingGrappled.isPullGrapple)
            {
                pointBeingGrappled.rb.transform.position = Vector2.Lerp(
                    grappleStartPos,
                    grappleStartPos + grappleVector,
                    t);
            }
            else
            {
                Rb.transform.position = Vector2.Lerp(
                    grappleStartPos,
                    grappleStartPos + grappleVector,
                    t);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
            grapplePullTimer += Time.deltaTime;
        }

        grappleVisuals.UnlinkPoint();
        SwitchStates(ProtagStates.Airborne);
    }

    public void FixedUpdateGrapplingState()
    {
    }

    private void TryGrapplingSwitch()
    {
        if (targetedGrapplePoint != null && grappleCooldownTimer <= 0f && !isGrappling && !isGrappleObstructed)
        {
            SwitchStates(ProtagStates.Grappling);
        }
    }
}