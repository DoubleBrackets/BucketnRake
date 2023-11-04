using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MovementProfile", fileName = "MovementProfile")]
public class MovementProfileSO : ScriptableObject
{
    [field: SerializeField]
    public float MaxSpeed { get; private set; }
    
    [field: SerializeField]
    public float GroundAcceleration { get; private set; }
    
    [field: SerializeField]
    public float GroundFriction { get; private set; }
    
    [field: SerializeField]
    public float JumpVelocity { get; private set; }
}
