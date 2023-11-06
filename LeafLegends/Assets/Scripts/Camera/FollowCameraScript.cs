using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FollowCameraScript : MonoBehaviour
{
    public static FollowCameraScript Instance { get; private set; }

    [SerializeField] private PlayerAnchorSO playerAnchor;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Header("Aiming config"), SerializeField]
    private float minOrthoWidth;

    [SerializeField] private float minOrthoHeight;
    [SerializeField] private float widthMargin;
    [SerializeField] private float heightMargin;
    [SerializeField] private float unitLerpFactor = 0.99f;

    [Header("Lookahead"), SerializeField]
    private float lookaheadFactor;

    [SerializeField] private float maxLookAhead;

    private Vector2 lastAveragePos;

    public HashSet<Transform> AdditionalTargets = new();

    private void Awake()
    {
        lastAveragePos = transform.position;
        Instance = this;
    }

    private void Update()
    {
        if (playerAnchor.RakePlayerController == null || playerAnchor.BucketPlayerController == null)
        {
            return;
        }

        Vector2 rakePos = playerAnchor.RakePlayerController.transform.position;
        Vector2 bucketPos = playerAnchor.BucketPlayerController.transform.position;

        // Calculate bounds
        var leftBound = Mathf.Min(bucketPos.x, rakePos.x);
        var rightBound = Mathf.Max(bucketPos.x, rakePos.x);

        var upperBound = Mathf.Max(bucketPos.y, rakePos.y);
        var lowerBound = Mathf.Min(bucketPos.y, rakePos.y);

        List<Transform> targets = new();
        
        foreach (var target in AdditionalTargets)
        {
            if (target == null)
            {
                targets.Add(target);
                continue;
            }
            var pos = target.position;
            leftBound = Mathf.Min(leftBound, pos.x);
            rightBound = Mathf.Max(rightBound, pos.x);

            upperBound = Mathf.Max(upperBound, pos.y);
            lowerBound = Mathf.Min(lowerBound, pos.y);
        }
        
        foreach (var target in targets)
        {
            AdditionalTargets.Remove(target);
        }

        leftBound -= widthMargin;
        rightBound += widthMargin;

        upperBound += heightMargin;
        lowerBound -= heightMargin;

        var width = Mathf.Max(minOrthoWidth, rightBound - leftBound);
        var height = Mathf.Max(minOrthoHeight, upperBound - lowerBound);

        // Decide on width driven or height driven
        var aspectRatio = (float)Screen.currentResolution.height / Screen.currentResolution.width;

        var heightFromWidth = width * aspectRatio;

        // Apply to virtual camera with smoothing
        var orthoSize = Mathf.Max(heightFromWidth, height);
        var averagePos = (rakePos + bucketPos) / 2;

        var currentOrthoSize = virtualCamera.m_Lens.OrthographicSize;
        Vector2 currentPos = virtualCamera.transform.position;

        var t = 1 - Mathf.Pow(1 - unitLerpFactor, Time.deltaTime);
        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(currentOrthoSize, orthoSize / 2, t);

        // Slight lookahead based on camera movement
        var delta = averagePos - lastAveragePos;
        var velocity = delta / Time.deltaTime;
        var lookahead = Vector2.ClampMagnitude(velocity * lookaheadFactor, maxLookAhead);

        Vector3 cameraPos = Vector2.Lerp(currentPos, averagePos + lookahead, t);
        cameraPos.z = -10;
        virtualCamera.transform.position = cameraPos;

        lastAveragePos = averagePos;
    }
}