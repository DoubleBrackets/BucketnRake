using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSFXPlayer : MonoBehaviour
{
    [SerializeField]
    private SFX sfxType;

    public void Play()
    {
        AudioManager.Instance.PlaySFX(sfxType, transform.position);
    }
}
