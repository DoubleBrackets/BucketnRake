using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField]
    private float openDuration;

    [SerializeField]
    private SpriteRenderer renderer;
    
    [SerializeField]
    private Sprite openSprite;

    [SerializeField]
    private Transform targetPos;

    [SerializeField]
    private Transform doorBody;

    [SerializeField]
    private AnimationCurve moveCurve;

    public void Open()
    {
        if(renderer)
            renderer.sprite = openSprite;
        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor()
    {
        AudioManager.Instance.PlaySFX(SFX.DoorOpen, transform.position);
        float time = 0f;
        Vector2 startPos = doorBody.position;
        Vector2 targetPos = this.targetPos.position;
        while (time < openDuration)
        {
            time += Time.deltaTime;
            float t = time / openDuration;
            doorBody.position = Vector2.Lerp(startPos, targetPos, moveCurve.Evaluate(t));
            yield return new WaitForEndOfFrame();
        }

        doorBody.position = targetPos;
    }
}
