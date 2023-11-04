using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GizmoThrottler : MonoBehaviour
{
    public static long GizmoTick;

    private void OnDrawGizmos()
    {
        GizmoTick++;
    }
}
