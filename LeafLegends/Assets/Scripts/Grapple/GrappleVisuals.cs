using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GrappleVisuals : MonoBehaviour
{
    [SerializeField]
    private LineRenderer targetIndicatorLineRenderer;

    [SerializeField]
    private LineRenderer grappleGrabLineRenderer;

    [SerializeField]
    private GameObject grappleProjectile;

    [SerializeField]
    private AnimationCurve waveStrengthCurve;

    [SerializeField]
    private float grappleFlightRatio;

    [Header("Grapple Projectile Animation"), SerializeField]
    private float waveStrength;

    [SerializeField] private float scrollSpeed;
    [SerializeField] private float freq;

    private GrapplePoint currentTarget;
    private GrapplePoint linkedPoint;

    private void Awake()
    {
        ClearTarget();
        UnlinkPoint();
        grappleProjectile.SetActive(false);
    }

    public void IndicateTarget(GrapplePoint target)
    {
        currentTarget = target;
        targetIndicatorLineRenderer.positionCount = 3;
    }

    public void ClearTarget()
    {
        currentTarget = null;
        targetIndicatorLineRenderer.positionCount = 0;
    }

    private void Update()
    {
        if (currentTarget != null)
        {
            Update2SegLine(targetIndicatorLineRenderer, transform.position, currentTarget.transform.position);
        }

        if (linkedPoint != null)
        {
            grappleProjectile.transform.position = linkedPoint.transform.position;
            grappleProjectile.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, linkedPoint.transform.position - transform.position));
            Update2SegLine(grappleGrabLineRenderer, transform.position, linkedPoint.transform.position);
        }
    }

    private void Update2SegLine(LineRenderer renderer, Vector3 p1, Vector3 p2)
    {
        renderer.SetPosition(0, p1);
        renderer.SetPosition(1, (p1 + p2) / 2);
        renderer.SetPosition(2, p2);
    }

    public async UniTask WindupAnimation(GrapplePoint point, float duration)
    {
        grappleProjectile.SetActive(true);
        var timer = 0f;
        var resolution = 50;
        grappleGrabLineRenderer.positionCount = resolution;

        Vector2 grappleVector = point.transform.position - transform.position;
        var dist = grappleVector.magnitude;

        Vector2 noiseDir = Quaternion.Euler(0, 0, 90) * grappleVector.normalized;

        while (timer < duration)
        {
            var t = timer / duration;
            // Spend half the time midair and half the time tightening
            var vec = grappleVector.normalized * (dist * Mathf.Min(1f, t * 1 / grappleFlightRatio));
            for (var i = 0; i < resolution; i++)
            {
                var pointT = (float)i / (resolution - 1);
                var pointPos = (Vector2)transform.position + vec * pointT;

                // Add some of the wavey rope noise
                var waveDistStrength = waveStrengthCurve.Evaluate(pointT);
                pointPos += noiseDir *
                            (Mathf.Sin(pointT * dist * freq + Time.time * scrollSpeed)
                             * (1 - t) * waveStrength * waveDistStrength);

                grappleGrabLineRenderer.SetPosition(i, pointPos);
            }

            // Update hook
            var deriv = new Vector2(1, Mathf.Cos(dist * freq + Time.time * scrollSpeed)).normalized;
            deriv = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, grappleVector)) * deriv;
            var lastPos = grappleGrabLineRenderer.GetPosition(resolution - 1);
            grappleProjectile.transform.position = lastPos;
            grappleProjectile.transform.rotation =
                Quaternion.Euler(0, 0,
                    Vector2.SignedAngle(
                        Vector2.right,
                        deriv));
            await UniTask.Yield(PlayerLoopTiming.Update);
            timer += Time.deltaTime;
        }

        grappleProjectile.SetActive(false);
        grappleGrabLineRenderer.positionCount = 0;
    }

    public void LinkPoint(GrapplePoint point)
    {
        grappleProjectile.SetActive(true);
        linkedPoint = point;
        grappleGrabLineRenderer.positionCount = 3;
    }

    public void UnlinkPoint()
    {
        grappleProjectile.SetActive(false);
        linkedPoint = null;
        grappleGrabLineRenderer.positionCount = 0;
    }
}