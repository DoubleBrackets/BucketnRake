using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharControllerConfig", fileName = "CharControllerConfig")]
public class CharController2DConfigSO : ScriptableObject
{
    [field: SerializeField]
    public CharController2DConfig Config;
}

[System.Serializable]
public class CharController2DConfig
{
    [Header("Airborne Config")]
    [SerializeField] public float GravityAccel;

    [SerializeField] public float AirbornHAcceleration;
    [SerializeField] public float AirbornHFriction;
    [SerializeField] public float MaxHAirbornSpeed;
    [SerializeField] public float JumpVelocity;


    [Header("Grounded Config")]
    [SerializeField] public float GroundedHAcceleration;

    [SerializeField] public float GroundedHFriction;
    [SerializeField] public float MaxGroundedHSpeed;


    [Header("Ground Check Config")]

    [SerializeField] public LayerMask GroundLayerMask;
    [SerializeField] public float LedgeSnapAngle = 45f;
    [SerializeField] public float StableOnGroundAngle = 45f;
    [SerializeField] public float GroundCastSizeReduction = 0.025f;
    [SerializeField] public float GroundCapsuleCastDistance  = 0.1f;
    [SerializeField] public float GroundRayCastDistance  = 0.25f;
    [SerializeField] public float GroundStickAccel;
}