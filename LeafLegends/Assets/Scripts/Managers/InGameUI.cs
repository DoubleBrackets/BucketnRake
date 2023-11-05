using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField]
    private Slider progressBar;

    [SerializeField]
    private TMP_Text progressText;

    [SerializeField]
    private LeafManager leafManager;

    private void Awake()
    {
        leafManager.OnLeavesChanged += UpdateLeavesProgress;
    }

    private void OnDestroy()
    {
        leafManager.OnLeavesChanged -= UpdateLeavesProgress;
    }

    private void UpdateLeavesProgress(int currentLeaves, int totalLeaves)
    {
        progressBar.value = (float)currentLeaves / totalLeaves;
        progressText.text = $"<b>{(float)currentLeaves / totalLeaves * 100:N0}%</b> Of Leaves Cleaned";
    }
}