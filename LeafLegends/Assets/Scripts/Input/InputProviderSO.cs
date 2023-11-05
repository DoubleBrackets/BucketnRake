using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "InputProvider")]
public class InputProviderSO : ScriptableObject
{
    public event Action OnJumpPressed;
    public event Action OnJumpReleased;

    [ShowInInspector]
    public float HorizontalAxis { get; set; }

    [ShowInInspector]
    public float VerticalAxis { get; set; }

    public void InvokeOnJumpPressed()
    {
        OnJumpPressed?.Invoke();
    }

    public void InvokeOnJumpReleased()
    {
        OnJumpReleased?.Invoke();
    }
}