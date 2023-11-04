using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class BucketAbility : MonoBehaviour
{
    public bool CanBucket { get; set; } = true;

    private void OnTriggerStay2D(Collider2D other)
    {
        TryBucket(other);
    }

    private void TryBucket(Collider2D coll)
    {
        if (!CanBucket)
        {
            return;
        }

        var leafInstance = coll.GetComponent<LeafInstance>();
        if (leafInstance)
        {
            leafInstance.TryCollectWithBucket();
        }
    }
}