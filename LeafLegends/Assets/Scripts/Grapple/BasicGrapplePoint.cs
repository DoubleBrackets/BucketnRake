using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGrapplePoint : GrapplePoint
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    public override void ApplyGrapple()
    {
        // Do nothing
    }

    public override void IsCurrentTarget(bool isTarget)
    {
        spriteRenderer.enabled = isTarget;
    }
}