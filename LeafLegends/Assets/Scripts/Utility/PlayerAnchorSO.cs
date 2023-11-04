using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerAnchor", fileName = "PlayerAnchor")]
public class PlayerAnchorSO : ScriptableObject
{
    [field: SerializeField]
    public ProtagController RakePlayerController { get; set; }
    
    [field: SerializeField]
    public ProtagController BucketPlayerController { get; set; }
}
