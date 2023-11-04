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
    void Update()
    {
        ReadRakePlayerInput();
        ReadBucketPlayerInput();
    }

    private void ReadBucketPlayerInput()
    {
        bucketPlayerInputProvider.HorizontalAxis = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bucketPlayerInputProvider.InvokeOnJumpPressed();
        }
    }

    private void ReadRakePlayerInput()
    {
        rakePlayerInputProvider.HorizontalAxis = (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            rakePlayerInputProvider.InvokeOnJumpPressed();
        }
    }
}
