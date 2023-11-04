using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    private int playerCount = 0;

    private void Awake()
    {
        virtualCamera.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount++;
            virtualCamera.enabled = playerCount >= 2;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount--;
            virtualCamera.enabled = playerCount >= 2;
        }
    }
}
