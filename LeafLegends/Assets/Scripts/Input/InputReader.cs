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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bucketPlayerInputProvider.InvokeOnJumpPressed();
        }

        if (Input.GetKeyUp(KeyCode.Space) )
        {
            bucketPlayerInputProvider.InvokeOnJumpReleased();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            bucketPlayerInputProvider.InvokeSpecialAbility();
        }
    }

    private void ReadRakePlayerInput()
    {
        rakePlayerInputProvider.HorizontalAxis = (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
        rakePlayerInputProvider.VerticalAxis = (Input.GetKey(KeyCode.DownArrow) ? -1 : 0) + (Input.GetKey(KeyCode.UpArrow) ? 1 : 0);

        if (Input.GetKeyDown(KeyCode.Question) || Input.GetKeyDown(KeyCode.RightControl))
        {
            rakePlayerInputProvider.InvokeOnJumpPressed();
        }

        if (Input.GetKeyUp(KeyCode.Question) || Input.GetKeyUp(KeyCode.RightControl))
        {
            rakePlayerInputProvider.InvokeOnJumpReleased();
        }
    }
}