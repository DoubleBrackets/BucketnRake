using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FollowCameraScript : MonoBehaviour
{
    [SerializeField] private PlayerAnchorSO playerAnchor;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    
    [Header("Aiming config")]
    [SerializeField] private float minOrthoWidth;
    [SerializeField] private float minOrthoHeight;
    [SerializeField] private float widthMargin;
    [SerializeField] private float heightMargin;
    [SerializeField] private float unitLerpFactor = 0.99f;

    [Header("Lookahead")]
    [SerializeField] private float lookaheadFactor;
    [SerializeField] private float maxLookAhead;

    private Vector2 lastAveragePos;

    private void Awake()
    {
        lastAveragePos = transform.position;
    }

    private void Update()
    {
        if (playerAnchor.RakePlayerController == null)
        {
            return;
        }
        
        Vector2 rakePos = playerAnchor.RakePlayerController.transform.position;
        Vector2 bucketPos = playerAnchor.BucketPlayerController.transform.position;

        // Calculate bounds
        float leftBound = Mathf.Min(bucketPos.x, rakePos.x) - widthMargin;
        float rightBound = Mathf.Max(bucketPos.x, rakePos.x) + widthMargin;

        float upperBound = Mathf.Max(bucketPos.y, rakePos.y) + heightMargin;
        float lowerBound = Mathf.Min(bucketPos.y, rakePos.y) - heightMargin;

        float width = Mathf.Max(minOrthoWidth, rightBound - leftBound);
        float height = Mathf.Max(minOrthoHeight, upperBound - lowerBound);

        // Decide on width driven or height driven
        float aspectRatio = (float)Screen.currentResolution.height / Screen.currentResolution.width;

        float heightFromWidth = width * aspectRatio;

        // Apply to virtual camera with smoothing
        float orthoSize = Mathf.Max(heightFromWidth, height);
        Vector2 averagePos = (rakePos + bucketPos) / 2;

        float currentOrthoSize = virtualCamera.m_Lens.OrthographicSize;
        Vector2 currentPos = virtualCamera.transform.position;
        
        float t = 1 - Mathf.Pow(1 - unitLerpFactor, Time.deltaTime);
        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(currentOrthoSize, orthoSize / 2, t);
        
        // Slight lookahead based on camera movement
        Vector2 delta = (averagePos - lastAveragePos);
        Vector2 velocity = delta / Time.deltaTime;
        Vector2 lookahead = Vector2.ClampMagnitude(velocity * lookaheadFactor, maxLookAhead);
        
        Vector3 cameraPos = Vector2.Lerp(currentPos, averagePos + lookahead, t);
        cameraPos.z = -10;
        virtualCamera.transform.position = cameraPos;

        lastAveragePos = averagePos;
    }
}
