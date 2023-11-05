using System;
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
    private SpriteRect spriteRen;

    private CharController2DConfig ControllerConfig => controllerConfigSO.Config;

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


    private void FixedUpdate()
    {
        charController.UpdateControllerState(ControllerConfig, currentMoveInput);

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

        currentMoveInput = new CharController2D.MoveInput
        {
            deltaTime = Time.fixedDeltaTime,
            horizontalInput = inputProvider.HorizontalAxis
        };

        if (rakeAbility)
        {
            rakeAbility.CanRake = Mathf.Abs(charController.Velocity.x) > 0.1f;
        }
    }
}