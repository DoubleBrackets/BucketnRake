using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RakeAbility : MonoBehaviour
{
    public bool CanRake { get; set; }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryRake(other);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        TryRake(col);
    }

    private void TryRake(Collider2D coll)
    {
        if (!CanRake)
        {
            return;
        }

        var leafInstance = coll.GetComponent<LeafInstance>();
        if (leafInstance)
        {
            leafInstance.TryRake();
        }
    }
}