using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Sirenix.OdinInspector;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ProtagController : MonoBehaviour
{

    [SerializeField] 
    private InputProviderSO inputProvider;

    [SerializeField]
    private CharController2DConfigSO controllerConfigSO;

    [SerializeField]
    private CharController2D charController;

    private CharController2DConfig ControllerConfig => controllerConfigSO.Config;
    
    private void Awake()
    {
        inputProvider.OnJumpPressed += OnJumpPressed;
    }

    private void OnDestroy()
    {
        inputProvider.OnJumpPressed -= OnJumpPressed;
    }

    private void OnJumpPressed()
    {
        if (charController.CurrentStateContext.stableOnGround)
        {
            charController.ForceAirborneTime = 0.2f;
            Vector2 vel = charController.Velocity;
            vel.y = ControllerConfig.JumpVelocity;
            charController.Velocity = vel;
        }
    }

    private void FixedUpdate()
    {
        CharController2D.MoveInput input = new CharController2D.MoveInput()
        {
            deltaTime = Time.fixedDeltaTime,
            horizontalInput = inputProvider.HorizontalAxis
        };
        charController.UpdateCharacterController(ControllerConfig, input);
    }
}
