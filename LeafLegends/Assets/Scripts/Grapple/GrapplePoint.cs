using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class GrapplePoint : MonoBehaviour
{
    public bool isPullGrapple;
    public Rigidbody2D rb;
    public CharController2D controller;

    private void Awake()
    {
        IsCurrentTarget(false);
    }

    public abstract void ApplyGrapple();

    public abstract void IsCurrentTarget(bool isTarget);
}