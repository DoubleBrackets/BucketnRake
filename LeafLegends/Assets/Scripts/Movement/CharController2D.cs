using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharController2D : MonoBehaviour
{
    [SerializeField]
    private CapsuleCollider2D collider;

    [SerializeField]
    private Rigidbody2D rb;

    public Vector2 Velocity
    {
        get => rb.velocity;
        set => rb.velocity = value;
    }

    public struct MoveInput
    {
        public float deltaTime;
        public float horizontalInput;
    }

    // The controller acts like a pipeline, with each step modifying the state context
    public struct StateContext
    {
        public bool touchingGround;
        public bool stableOnGround;
        
        public Vector2 groundNormal;

        public readonly float deltaTime;
        
        // Ground cast using the capsule collider
        public RaycastHit2D groundCapsuleCastHit;
        
        // Ground cast using a raycast from the center for ground snap testing
        // This is to prevent snapping to sharp edges
        public RaycastHit2D groundRaycastHit;

        public Vector2 transientVelocity;

        public StateContext(float deltaTime) : this()
        {
            this.deltaTime = deltaTime;
        }
    }

    [ShowInInspector]
    private StateContext currentStateContext;

    public StateContext CurrentStateContext => currentStateContext;

    public float ForceAirborneTime
    {
        get => forceAirborneTime;
        set => forceAirborneTime = value;
    }

    private float forceAirborneTime;

    public void UpdateCharacterController(CharController2DConfig config, MoveInput moveInput)
    {
        ForceAirborneTime -= moveInput.deltaTime;
        
        // Set up context
        StateContext evalMoveContext = new StateContext(moveInput.deltaTime);
        evalMoveContext.transientVelocity = rb.velocity;
        
        // Calculate context for the physics tick
        GroundCast(config, ref evalMoveContext);
        EvaluateGroundState(config, ref evalMoveContext);
        
        HorizontalMovement(config, moveInput, ref evalMoveContext);
        VerticalMovement(config, ref evalMoveContext);

        // Apply context back to rb
        rb.velocity = evalMoveContext.transientVelocity;
        currentStateContext = evalMoveContext;
    }

    private void HorizontalMovement(in CharController2DConfig config, in MoveInput moveInput, ref StateContext stateContext)
    {
        // Calculate desired velocity and acceleration
        float maxSpeed = stateContext.stableOnGround ? config.MaxGroundedHSpeed : config.MaxHAirbornSpeed;
        float desiredHorizontalVelocity = moveInput.horizontalInput * maxSpeed;
        
        float activeAccel = stateContext.stableOnGround ? config.GroundedHAcceleration : config.AirbornHAcceleration;
        float frictionAccel = stateContext.stableOnGround ? config.GroundedHFriction : config.AirbornHFriction;

        float finalAccel = activeAccel;
        // If trying to stand still, use friction
        if(Mathf.Abs(desiredHorizontalVelocity) == 0)
        {
            finalAccel = frictionAccel;
        }
        
        // If moving in opposite direction, then take the largest between friction and acceleration
        if (desiredHorizontalVelocity * stateContext.transientVelocity.x < 0)
        {
            finalAccel = Mathf.Max(activeAccel, frictionAccel);
        }
        
        // Calculate new velocity
        float groundAngle = Vector2.Angle(stateContext.groundNormal, Vector2.up);

        // Transform velocity to "ground" tangent space
        Vector2 relativeVelocity = Quaternion.Euler(0, 0, -groundAngle) * stateContext.transientVelocity;

        relativeVelocity.x = Mathf.MoveTowards(
            relativeVelocity.x,
            desiredHorizontalVelocity,
            finalAccel * stateContext.deltaTime);
        
        // Transform velocity back to world space and apply
        stateContext.transientVelocity = Quaternion.Euler(0, 0, groundAngle) * relativeVelocity;
    }
    
    private void VerticalMovement(in CharController2DConfig config, ref StateContext stateContext)
    {
        // Only apply gravity if unstable
        if (!stateContext.stableOnGround)
        {
            stateContext.transientVelocity.y -= config.GravityAccel * stateContext.deltaTime;
        }
    }

    private void GroundCast(in CharController2DConfig config, ref StateContext evalMoveContext)
    {
        var size = collider.size;
        size.y -= config.GroundCastSizeReduction;
        
        evalMoveContext.groundCapsuleCastHit = Physics2D.CapsuleCast(
            collider.bounds.center, 
            size, 
            CapsuleDirection2D.Vertical, 
            0, 
            Vector2.down, 
            config.GroundCapsuleCastDistance + config.GroundCastSizeReduction / 2,
            config.GroundLayerMask);

        evalMoveContext.groundRaycastHit = Physics2D.Raycast(
            collider.bounds.center,
            Vector2.down,
            collider.size.y / 2 + config.GroundRayCastDistance,
            config.GroundLayerMask
        );
    }

    private void EvaluateGroundState(in CharController2DConfig config, ref StateContext evalMoveContext)
    {
        var hit = evalMoveContext.groundCapsuleCastHit;
        // Check if ground is present
        if (hit.collider == null || ForceAirborneTime > 0)
        {
            evalMoveContext.touchingGround = false;
            evalMoveContext.stableOnGround = false;
            evalMoveContext.groundNormal = Vector2.up;
            return;
        }
        evalMoveContext.touchingGround = true;
        
        // Check if ground is stable
        evalMoveContext.groundNormal = hit.normal;
        float groundSlopeAngle = Vector2.Angle(hit.normal, Vector2.up);
        
        if(groundSlopeAngle <= config.StableOnGroundAngle)
        {
            evalMoveContext.stableOnGround = true;
            SnapToGround(config, ref evalMoveContext);
        }
        else
        {
            evalMoveContext.stableOnGround = false;
        }
    }

    private void SnapToGround(in CharController2DConfig config, ref StateContext evalMoveContext)
    {
        // If the change in ground angle is too steep and we're moving towards a ledge, then don't snap and force airborne
        // Basically launching over a ledge
        // Also assumes a ledge if the raycast doesn't hit anything
        float changeAngle = Vector2.Angle(evalMoveContext.groundRaycastHit.normal, currentStateContext.groundRaycastHit.normal);
        bool movingAwayFromSlope = Vector2.Dot(evalMoveContext.transientVelocity, evalMoveContext.groundRaycastHit.normal) > 0;
        if(evalMoveContext.groundRaycastHit.collider == null || changeAngle > config.LedgeSnapAngle && movingAwayFromSlope)
        {
            ForceAirborneTime = 0.1f;
            return;
        }
        
        // If we're moving away from the ground, then remove all relative vertical velocity 
        if (Vector2.Dot(evalMoveContext.groundNormal, evalMoveContext.transientVelocity) > 0)
        {
            evalMoveContext.transientVelocity = Vector3.ProjectOnPlane(evalMoveContext.transientVelocity, evalMoveContext.groundNormal);
        }
        
        // Stick to ground with some force
        evalMoveContext.transientVelocity -= evalMoveContext.groundNormal * (config.GroundStickAccel * evalMoveContext.deltaTime);
    }

}
