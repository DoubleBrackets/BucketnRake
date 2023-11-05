using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericLever : MonoBehaviour
{
    public UnityEvent OnLeverPulled;

    public SimpleAnimator Animator;

    private bool pulled;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !pulled)
        {
            pulled = true;
            OnLeverPulled?.Invoke();
            AudioManager.Instance.PlaySFX(SFX.Lever, transform.position);
            Animator.Play();
        }
    }
}