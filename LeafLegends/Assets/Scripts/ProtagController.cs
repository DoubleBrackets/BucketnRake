using UnityEngine;

public class ProtagController : MonoBehaviour
{
    [SerializeField]
    private InputProviderSO inputProvider;

    [SerializeField]
    private CharController2DConfigSO controllerConfigSO;

    [SerializeField]
    private CharController2D charController;

    [SerializeField]
    private RakeAbility rakeAbility;

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
            var vel = charController.Velocity;
            vel.y = ControllerConfig.JumpVelocity;
            charController.Velocity = vel;
        }
    }

    private void FixedUpdate()
    {
        var input = new CharController2D.MoveInput
        {
            deltaTime = Time.fixedDeltaTime,
            horizontalInput = inputProvider.HorizontalAxis
        };

        if (rakeAbility)
        {
            rakeAbility.CanRake = inputProvider.HorizontalAxis != 0;
        }

        charController.UpdateCharacterController(ControllerConfig, input);
    }
}