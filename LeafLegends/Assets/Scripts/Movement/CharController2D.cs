using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

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

        public StateContext(float deltaTime) : this() => this.deltaTime = deltaTime;
    }

    [ShowInInspector]
    private StateContext currentStateContext;

    private StateContext prevStateContext;

    public StateContext CurrentStateContext => currentStateContext;
    public StateContext PrevStateContext => prevStateContext;


    private float forceAirborneTime;
    public float GravityScale { get; set; } = 1f;
    public float CoyoteTime { get; set; }

    public void ForceAirborne(float time)
    {
        currentStateContext.stableOnGround = false;
        forceAirborneTime = time;
    }

    public void OffsetCapsuleHeight(float heightChange)
    {
        var colliderSize = collider.size;
        colliderSize.y += heightChange;
        collider.size = colliderSize;

        collider.offset = new Vector2(collider.offset.x, collider.offset.y + heightChange / 2);
    }

    public void UpdateControllerState(CharController2DConfig config, MoveInput moveInput)
    {
        forceAirborneTime -= moveInput.deltaTime;
        CoyoteTime += moveInput.deltaTime;

        // Set up context
        var evalMoveContext = new StateContext(moveInput.deltaTime);

        // Calculate context for the physics tick
        GroundCast(config, ref evalMoveContext);
        EvaluateGroundState(config, ref evalMoveContext);

        // Update state
        prevStateContext = currentStateContext;
        currentStateContext = evalMoveContext;
    }

    public void HorizontalMovement(in CharController2DConfig config, in MoveInput moveInput)
    {
        var transientVelocity = rb.velocity;

        // Calculate desired velocity and acceleration
        var maxSpeed = currentStateContext.stableOnGround ? config.MaxGroundedHSpeed : config.MaxHAirbornSpeed;
        var desiredHorizontalVelocity = moveInput.horizontalInput * maxSpeed;

        var activeAccel = currentStateContext.stableOnGround ? config.GroundedHAcceleration : config.AirbornHAcceleration;
        var frictionAccel = currentStateContext.stableOnGround ? config.GroundedHFriction : config.AirbornHFriction;

        var finalAccel = activeAccel;
        // If trying to stand still, use friction
        if (Mathf.Abs(desiredHorizontalVelocity) == 0)
        {
            finalAccel = frictionAccel;
        }

        // If moving in opposite direction or trying to slow down, then use friction
        if (desiredHorizontalVelocity * transientVelocity.x < 0)
        {
            finalAccel = Mathf.Max(frictionAccel, activeAccel);
        }

        // Calculate new velocity
        var groundAngle = Vector2.SignedAngle(Vector2.up, currentStateContext.groundNormal);

        // Transform velocity to "ground" tangent space
        Vector2 relativeVelocity = Quaternion.Euler(0, 0, -groundAngle) * transientVelocity;

        relativeVelocity.x = Mathf.MoveTowards(
            relativeVelocity.x,
            desiredHorizontalVelocity,
            finalAccel * currentStateContext.deltaTime);

        // Transform velocity back to world space and apply
        transientVelocity = Quaternion.Euler(0, 0, groundAngle) * relativeVelocity;

        rb.velocity = transientVelocity;
    }

    public void AirbornePhysics(in CharController2DConfig config)
    {
        var transientVelocity = rb.velocity;

        // Only apply gravity if unstable
        if (!currentStateContext.stableOnGround)
        {
            transientVelocity.y -= config.GravityAccel * currentStateContext.deltaTime * GravityScale;
        }

        rb.velocity = transientVelocity;
    }

    private void GroundCast(in CharController2DConfig config, ref StateContext evalMoveContext)
    {
        evalMoveContext.groundCapsuleCastHit = CastCapsule(
            0,
            config.GroundCastSizeReduction,
            Vector2.down,
            config.GroundCapsuleCastDistance + config.GroundCastSizeReduction / 2,
            config.GroundLayerMask
        );

        evalMoveContext.groundRaycastHit = Physics2D.Raycast(
            collider.bounds.center,
            Vector2.down,
            collider.size.y / 2 + config.GroundRayCastDistance,
            config.GroundLayerMask
        );
    }

    public RaycastHit2D CastCapsule(
        float xReduction,
        float yReduction,
        Vector2 direction,
        float distance,
        LayerMask mask)
    {
        var size = collider.size;
        size.y -= yReduction;
        size.x -= xReduction;
        return Physics2D.CapsuleCast(
            collider.bounds.center,
            size,
            CapsuleDirection2D.Vertical,
            0,
            direction,
            distance,
            mask);
    }

    public Collider2D[] OverlapCapsule(Vector2 offset, float xReduction, float yReduction, LayerMask mask)
    {
        var size = collider.size;
        size.y -= yReduction;
        size.x -= xReduction;

        return Physics2D.OverlapCapsuleAll(
            collider.bounds.center + (Vector3)offset,
            size,
            CapsuleDirection2D.Vertical,
            0f,
            mask);
    }

    private void EvaluateGroundState(in CharController2DConfig config, ref StateContext evalMoveContext)
    {
        var hit = evalMoveContext.groundCapsuleCastHit;
        // Check if ground is present
        if (hit.collider == null || forceAirborneTime > 0)
        {
            evalMoveContext.touchingGround = false;
            evalMoveContext.stableOnGround = false;
            evalMoveContext.groundNormal = Vector2.up;
            return;
        }

        evalMoveContext.touchingGround = true;

        // Check if ground is stable
        evalMoveContext.groundNormal = hit.normal;
        Debug.Log($"{hit.normal}");
        var groundSlopeAngle = Vector2.Angle(hit.normal, Vector2.up);

        if (groundSlopeAngle <= config.StableOnGroundAngle)
        {
            evalMoveContext.stableOnGround = true;
            CoyoteTime = 0f;
        }
        else
        {
            evalMoveContext.groundNormal = Vector2.up;
            evalMoveContext.stableOnGround = false;
        }
    }

    public void SnapToGround(in CharController2DConfig config)
    {
        var transientVelocity = rb.velocity;

        // If the change in ground angle is too steep and we're moving towards a ledge, then don't snap and force airborne
        // Basically launching over a ledge
        // Also assumes a ledge if the raycast doesn't hit anything
        var changeAngle = Vector2.Angle(currentStateContext.groundRaycastHit.normal, currentStateContext.groundRaycastHit.normal);
        var movingAwayFromSlope = Vector2.Dot(transientVelocity, currentStateContext.groundRaycastHit.normal) >= 0;
        if ((currentStateContext.groundRaycastHit.collider == null || changeAngle > config.LedgeSnapAngle) && movingAwayFromSlope)
        {
            ForceAirborne(0.1f);
            return;
        }

        // If we're moving away from the ground, then remove all relative vertical velocity 
        if (Vector2.Dot(currentStateContext.groundNormal, transientVelocity) > 0)
        {
            transientVelocity = Vector3.ProjectOnPlane(transientVelocity, currentStateContext.groundNormal);
        }

        // Stick to ground with some force
        transientVelocity -= currentStateContext.groundNormal * (config.GroundStickAccel * currentStateContext.deltaTime);

        rb.velocity = transientVelocity;
    }
}