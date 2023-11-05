using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class ProtagController : MonoBehaviour
{
    [SerializeField]
    private InputProviderSO inputProvider;

    [SerializeField]
    private CharController2DConfigSO controllerConfigSO;

    [SerializeField]
    private CharController2D charController;

    [SerializeField]
    private RakeAbility rakeAbility;

    [SerializeField]
    private GrappleVisuals grappleVisuals;

    [SerializeField]
    private Transform rotatedBody;

    [SerializeField]
    private Animator animator;

    private CharController2DConfig ControllerConfig => controllerConfigSO.Config;
    private Rigidbody2D Rb => charController.Rb;

    private CharController2D.MoveInput currentMoveInput;

    public enum ProtagStates
    {
        Idle,
        Running,
        Airborne,
        JumpState,
        WallSlide,
        Grappling,
        Sliding
    }

    private ProtagStates currentState;

    private void OnEnable()
    {
        currentState = ProtagStates.Idle;
    }

    private void OnDisable()
    {
        ExitCurrentState();
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.yellow;
        Handles.Label(transform.position + Vector3.up * 2f, currentState.ToString());
    }

    private bool SwitchStates(ProtagStates newState)
    {
        if (newState == currentState)
        {
            return false;
        }

        ExitCurrentState();
        currentState = newState;
        switch (currentState)
        {
            case ProtagStates.Idle:
                EnterIdleState();
                break;
            case ProtagStates.Running:
                EnterRunningState();
                break;
            case ProtagStates.Airborne:
                EnterAirborneState();
                break;
            case ProtagStates.JumpState:
                EnterJumpState();
                break;
            case ProtagStates.WallSlide:
                EnterWallSlideState();
                break;
            case ProtagStates.Grappling:
                EnterGrapplingState();
                break;
            case ProtagStates.Sliding:
                EnterCrouchState();
                break;
        }

        return true;
    }

    private void ExitCurrentState()
    {
        switch (currentState)
        {
            case ProtagStates.Idle:
                ExitIdleState();
                break;
            case ProtagStates.Running:
                ExitRunningState();
                break;
            case ProtagStates.Airborne:
                ExitAirborneState();
                break;
            case ProtagStates.JumpState:
                ExitJumpState();
                break;
            case ProtagStates.WallSlide:
                ExitWallSlideState();
                break;
            case ProtagStates.Grappling:
                ExitGrapplingState();
                break;
            case ProtagStates.Sliding:
                ExitCrouchState();
                break;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case ProtagStates.Idle:
                UpdateIdleState();
                break;
            case ProtagStates.Running:
                UpdateRunningState();
                break;
            case ProtagStates.Airborne:
                UpdateAirborneState();
                break;
            case ProtagStates.JumpState:
                UpdateJumpState();
                break;
            case ProtagStates.WallSlide:
                UpdateWallSlideState();
                break;
            case ProtagStates.Grappling:
                UpdateGrapplingState();
                break;
            case ProtagStates.Sliding:
                UpdateCrouchState();
                break;
        }
    }

    private void FixedUpdateStates()
    {
        switch (currentState)
        {
            case ProtagStates.Idle:
                FixedUpdateIdleState();
                break;
            case ProtagStates.Running:
                FixedUpdateRunningState();
                break;
            case ProtagStates.Airborne:
                FixedUpdateAirborneState();
                break;
            case ProtagStates.JumpState:
                FixedUpdateJumpState();
                break;
            case ProtagStates.WallSlide:
                FixedUpdateWallSlideState();
                break;
            case ProtagStates.Grappling:
                FixedUpdateGrapplingState();
                break;
            case ProtagStates.Sliding:
                FixedUpdateCrouchState();
                break;
        }
    }

    private void FixedUpdate()
    {
        grappleCooldownTimer -= Time.deltaTime;
        slideCooldownTimer -= Time.deltaTime;

        currentMoveInput = new CharController2D.MoveInput
        {
            deltaTime = Time.fixedDeltaTime,
            horizontalInput = inputProvider.HorizontalAxis
        };

        charController.UpdateControllerState(ControllerConfig, currentMoveInput);

        FixedUpdateStates();

        if (rakeAbility)
        {
            rakeAbility.CanRake = charController.Velocity.sqrMagnitude > 0.1f;
        }

        if (ControllerConfig.CanGrapple)
        {
            TrySearchForGrapplePoints();
        }

        UpdateSpriteScaleAndRotation();
    }

    private void UpdateSpriteScaleAndRotation()
    {
        var scale = rotatedBody.localScale;
        if (currentMoveInput.horizontalInput != 0)
        {
            scale.x = currentMoveInput.horizontalInput;
        }

        rotatedBody.localScale = scale;

        if (charController.CurrentStateContext.stableOnGround)
        {
            var currentRot = Mathf.DeltaAngle(0, rotatedBody.rotation.eulerAngles.z);
            var targetRot = Vector2.SignedAngle(Vector2.up, charController.CurrentStateContext.rayGroundNormal);

            var t = 1 - Mathf.Pow(1 - 0.99f, Time.deltaTime * 10f);

            var newRot = Mathf.Lerp(currentRot, targetRot, t);
            rotatedBody.rotation = Quaternion.Euler(
                0,
                0,
                newRot
            );
        }
        else
        {
            rotatedBody.rotation = Quaternion.identity;
        }
    }

    private void TrySearchForGrapplePoints()
    {
        if (targetedGrapplePoint != null)
        {
            targetedGrapplePoint.IsCurrentTarget(false);
            grappleVisuals.ClearTarget();
            FollowCameraScript.Instance.AdditionalTargets.Remove(targetedGrapplePoint.transform);
            targetedGrapplePoint = null;
        }

        if (isGrappling || grappleCooldownTimer > 0f)
        {
            return;
        }

        // Find all points in the radius
        var colliders = Physics2D.OverlapCircleAll(transform.position, ControllerConfig.GrappleRadius, ControllerConfig.GrappleLayerMask);

        var inputVector = new Vector2(inputProvider.HorizontalAxis, inputProvider.VerticalAxis);

        // Choose the point that aligns to the input vector the most
        GrapplePoint bestCandidate = null;
        var closestDist = float.MaxValue;
        var closestSector = int.MaxValue;

        foreach (var coll in colliders)
        {
            var grapplePoint = coll.GetComponent<GrapplePoint>();

            if (grapplePoint == null)
            {
                continue;
            }

            // Raycast check if any terrain is in the way
            Vector2 delta = grapplePoint.transform.position - transform.position;
            var cast = Physics2D.Raycast(
                transform.position,
                delta,
                delta.magnitude,
                ControllerConfig.GroundLayerMask);

            if (cast.collider != null)
            {
                continue;
            }

            var angleSector = (int)(Vector2.Angle(delta, inputVector) / 45);
            var dist = delta.magnitude;

            if (inputVector == Vector2.zero)
            {
                angleSector = 0;
            }

            // Choose the point that is in the same 45 degree cone, and is closest
            if (angleSector < closestSector)
            {
                closestSector = angleSector;
                closestDist = dist;
                bestCandidate = grapplePoint;
            }
            else if (angleSector == closestSector && dist < closestDist)
            {
                closestDist = dist;
                bestCandidate = grapplePoint;
            }
        }

        if (bestCandidate != null)
        {
            targetedGrapplePoint = bestCandidate;
            targetedGrapplePoint.IsCurrentTarget(true);
            grappleVisuals.IndicateTarget(bestCandidate);
            FollowCameraScript.Instance.AdditionalTargets.Add(targetedGrapplePoint.transform);
        }
    }
}