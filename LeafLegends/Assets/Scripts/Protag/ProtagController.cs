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
        #if UNITY_EDITOR
        Handles.color = Color.yellow;
        Handles.Label(transform.position + Vector3.up * 2f, currentState.ToString());
        #endif
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

        if (ControllerConfig.CanGrapple)
        {
            UpdateGrappleTargetting();
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

    private List<GrapplePoint> inRangeGrapplePoints = new();
    private GrapplePoint targetedGrapplePoint;
    private bool isGrappleObstructed;

    private void UpdateGrappleTargetting()
    {
        // We recalculate every frame, so just clear everything at the start
        if (targetedGrapplePoint != null)
        {
            targetedGrapplePoint.IsCurrentTarget(false);
            grappleVisuals.ClearTarget();
            FollowCameraScript.Instance.AdditionalTargets.Remove(targetedGrapplePoint.transform);
        }
        
        inRangeGrapplePoints.Clear();

        // Get points in range
        GetAllGrapplePoints();
        
        if (inRangeGrapplePoints.Count == 0)
            return;

        bool currentPointInTargeted = inRangeGrapplePoints.Contains(targetedGrapplePoint);
        // If the current target is not in range, then clear it from current target
        if (targetedGrapplePoint != null && !currentPointInTargeted)
        {
            targetedGrapplePoint = null;
        }
        
        // Shift targets
        if (inputProvider.GrappleTargetPressed)
        {
            if (currentPointInTargeted)
            {
                int grappleIndex = inRangeGrapplePoints.FindIndex(a => a == targetedGrapplePoint);
                grappleIndex = (grappleIndex + 1) % inRangeGrapplePoints.Count;
                targetedGrapplePoint = inRangeGrapplePoints[grappleIndex];
            }
            else
            {
                targetedGrapplePoint = inRangeGrapplePoints[0];
            }
        }

        if (targetedGrapplePoint == null)
            return;
        
        // Indicate the current target
        targetedGrapplePoint.IsCurrentTarget(true);
        FollowCameraScript.Instance.AdditionalTargets.Add(targetedGrapplePoint.transform);

        if (isGrappling || grappleCooldownTimer > 0f)
        {
            return;
        }
        
        if (targetedGrapplePoint != null)
        {
            // Raycast check if any terrain is in the way
            Vector2 delta = targetedGrapplePoint.transform.position - transform.position;
            var cast = Physics2D.Raycast(
                transform.position,
                delta,
                delta.magnitude,
                ControllerConfig.GroundLayerMask);

            if (cast.collider == null)
            {
                grappleVisuals.IndicateTarget(targetedGrapplePoint);
                isGrappleObstructed = false;
            }
            else
            {
                isGrappleObstructed = true;
            }
        }
    }

    private void GetAllGrapplePoints()
    {
        // Find all points in the radius
        var colliders = Physics2D.OverlapCircleAll(transform.position, ControllerConfig.GrappleRadius, ControllerConfig.GrappleLayerMask);

        foreach (var coll in colliders)
        {
            var grapplePoint = coll.GetComponent<GrapplePoint>();

            if (grapplePoint == null)
            {
                continue;
            }

            inRangeGrapplePoints.Add(grapplePoint);
        }
        
        // Sort based on distance
        inRangeGrapplePoints.Sort((point1, point2) =>
        {
            var position = transform.position;
            float dist1 = Vector2.Distance(position, point1.transform.position);
            float dist2 = Vector2.Distance(position, point2.transform.position);
            return dist1.CompareTo(dist2);
        });
    }
}