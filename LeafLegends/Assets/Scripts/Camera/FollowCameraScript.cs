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

    private void Update()
    {
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

        float orthoSize = Mathf.Max(heightFromWidth, height);
        virtualCamera.m_Lens.OrthographicSize = orthoSize;
    }
}
