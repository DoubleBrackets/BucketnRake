using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class LeafProjector : MonoBehaviour
{
    [Header("Projection Config")]
    [SerializeField] private float spreadLength;
    [SerializeField] private float spreadAngle;
    [SerializeField] private float projectionSpacing;
    [SerializeField] private float projectionRadius;
    [SerializeField] private float projectionAngle;
    [SerializeField] private float projectionOffset;
    [SerializeField] private LayerMask projectionHitMask;

    [Header("Leaf Config")]
    [SerializeField] private float liftAngle;
    [SerializeField] private float liftDistance;

    [SerializeField] private LeafInstance leafInstance;

    private ProjectInfo[] projectionHitsInfo;

    struct ProjectInfo
    {
        public Vector2 startPos;
        public Vector2 hitPos;
        public Vector2 hitNormal;
        public bool didHit;
    }

    private void Start()
    {
        projectionHitsInfo = GetProjectionStartPos();
        GenerateInstances();
    }

    private void GenerateInstances()
    {
        foreach(var projectionInfo in projectionHitsInfo)
        {
            var createdLeafInstance = Instantiate(
                leafInstance, 
                projectionInfo.hitPos, 
                Quaternion.Euler(0, 0, Vector2.Angle(Vector2.up, projectionInfo.hitNormal)),
                transform);
            createdLeafInstance.Initialize(liftAngle, liftDistance);
        }
    }

    private void OnDrawGizmos()
    {
        if(GizmoThrottler.GizmoTick % 10 == 0)
        {
            projectionHitsInfo = GetProjectionStartPos();
        }
        
        if (projectionHitsInfo == null || Application.isPlaying)
        {
            return;
        }
        
        Vector3 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * Vector2.right;
        var startPos = transform.position;
        
        // Draw spread direction
        Gizmos.color = Color.green;
        Gizmos.DrawLine(startPos, startPos + spreadDirection * spreadLength);
        
        // Draw projections
        foreach (var projInfo in projectionHitsInfo)
        {
            if (!projInfo.didHit)
                continue;
            // Draw leaf projection point
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(projInfo.hitPos, 0.1f);
            Gizmos.DrawLine(projInfo.startPos, projInfo.hitPos);
                
            // Draw how far the leaves will rise on hit
            Gizmos.color = new Color(252/255f, 186/255f, 3/255f);
            Gizmos.DrawLine(projInfo.hitPos, projInfo.hitPos + (Vector2)(Quaternion.Euler(0,0,liftAngle) * Vector2.up * liftDistance));
        }
    }

    private ProjectInfo[] GetProjectionStartPos()
    { 
        int projectionCount = (int)(spreadLength / projectionSpacing);
        float adjustedSpreadLength = projectionCount * projectionSpacing;
        
        var projectionHits = new ProjectInfo[projectionCount];
        Vector2 spreadDir = Quaternion.Euler(0, 0, spreadAngle) * Vector2.right;
        Vector2 projectDir = Quaternion.Euler(0,0,projectionAngle) * Vector2.right;
        for (int i = 0; i < projectionCount; i++)
        {
            var projInfo = new ProjectInfo();
            float offset = (float)i / (projectionCount - 1) * adjustedSpreadLength;
            projInfo.startPos = transform.position + (Vector3)spreadDir * offset;
            
            RaycastHit2D hit = Physics2D.CircleCast(projInfo.startPos, projectionRadius, projectDir, 1000f, projectionHitMask);
            if (hit.collider)
            {
                projInfo.didHit = true;
                projInfo.hitNormal = hit.normal;
                projInfo.hitPos = projInfo.startPos + (hit.distance - projectionOffset) * projectDir ;
            }
            else
            {
                projInfo.didHit = false;
            }

            projectionHits[i] = projInfo;
        }

        return projectionHits;
    }
    
    [Button("Set Lift Opposite to Projection")]
    private void SetLiftOppositeToProjection()
    {
        liftAngle = projectionAngle + 90;
    }
}
