using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafInstance : MonoBehaviour
{
    private Vector2 liftDirection;
    private float liftDistance;
    
    public void Initialize(float liftAngle, float liftDistance)
    {
        liftDirection = (Vector2)((Quaternion.Euler(0, 0, liftAngle) * Vector2.up));
        this.liftDistance = liftDistance;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)liftDirection * liftDistance);
    }
}
