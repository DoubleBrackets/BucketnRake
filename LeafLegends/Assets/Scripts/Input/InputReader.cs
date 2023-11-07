using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputReader : MonoBehaviour
{
    [SerializeField]
    private InputProviderSO rakePlayerInputProvider;

    [SerializeField]
    private InputProviderSO bucketPlayerInputProvider;

    // Update is called once per frame
    private void Update()
    {
        ReadRakePlayerInput();
        ReadBucketPlayerInput();
    }

    private void ReadBucketPlayerInput()
    {
        bucketPlayerInputProvider.HorizontalAxis = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        bucketPlayerInputProvider.VerticalAxis = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            bucketPlayerInputProvider.InvokeOnJumpPressed();
        }

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Space))
        {
            bucketPlayerInputProvider.InvokeOnJumpReleased();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            bucketPlayerInputProvider.InvokeSpecialAbility(); 
        }

        bucketPlayerInputProvider.GrappleTargetPressed = Input.GetKeyDown(KeyCode.LeftShift);
    }

    private void ReadRakePlayerInput()
    {
        rakePlayerInputProvider.HorizontalAxis = (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
        rakePlayerInputProvider.VerticalAxis = (Input.GetKey(KeyCode.DownArrow) ? -1 : 0) + (Input.GetKey(KeyCode.UpArrow) ? 1 : 0);

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyUp(KeyCode.Question))
        {
            rakePlayerInputProvider.InvokeOnJumpPressed();
        }

        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.RightControl) || Input.GetKeyUp(KeyCode.Question))
        {
            rakePlayerInputProvider.InvokeOnJumpReleased();
        }
    }
}