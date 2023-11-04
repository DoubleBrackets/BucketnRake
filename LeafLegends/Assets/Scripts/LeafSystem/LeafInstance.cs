using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class LeafInstance : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private AnimationCurve rakeMotionCurve;

    [SerializeField]
    private float rakeMotionDuration;

    [SerializeField]
    private float distanceVariation;

    [SerializeField]
    private float rakeHorizontalVelVariation;

    [SerializeField]
    private float rakeAngleVelVariation;

    [SerializeField]
    private float defaultRakeDistance;

    private Vector2 rakeDirection;
    private float rakeDistance;

    private bool raked;
    private bool isRaking;

    public void Initialize(float rakeAngle, float rakeDistance)
    {
        raked = false;
        rb.bodyType = RigidbodyType2D.Static;
        rakeDirection = (Vector2)(Quaternion.Euler(0, 0, rakeAngle) * Vector2.up);
        this.rakeDistance = rakeDistance + Random.Range(-distanceVariation, distanceVariation);
    }

    public bool TryRake()
    {
        if (isRaking)
        {
            return false;
        }

        if (!raked)
        {
            PerformRake(gameObject.GetCancellationTokenOnDestroy());
            return true;
        }
        else if (rb.GetContacts(new ContactPoint2D[1]) > 0)
        {
            PerformRake(gameObject.GetCancellationTokenOnDestroy());
            return true;
        }

        return false;
    }

    private async UniTaskVoid PerformRake(CancellationToken token)
    {
        isRaking = true;
        var time = 0f;
        Vector2 startPos = transform.position;
        while (time < rakeMotionDuration)
        {
            var t = time / rakeMotionDuration;
            await UniTask.Yield(PlayerLoopTiming.Update);
            if (token.IsCancellationRequested)
            {
                return;
            }

            time += Time.deltaTime;
            transform.position = startPos + rakeMotionCurve.Evaluate(t) * rakeDirection * rakeDistance;
        }

        if (!raked)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rakeDistance = defaultRakeDistance + Random.Range(-distanceVariation, distanceVariation);
            rakeDirection = Vector2.up;
            raked = true;
        }

        // Add some variation to the motion
        rb.angularVelocity = Random.Range(-rakeAngleVelVariation, rakeAngleVelVariation);
        var vel = rb.velocity;
        vel.x += Random.Range(-rakeHorizontalVelVariation, rakeHorizontalVelVariation);
        rb.velocity = vel;
        isRaking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rakeDirection * rakeDistance);
    }

    public bool TryCollectWithBucket()
    {
        // can't collect leaves on the ground
        if (rb.GetContacts(new ContactPoint2D[1]) > 0)
        {
            return false;
        }

        gameObject.SetActive(false);
        return true;
    }
}