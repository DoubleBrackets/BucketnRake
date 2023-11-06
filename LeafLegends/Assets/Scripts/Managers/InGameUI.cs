using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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

    [SerializeField]
    private CanvasGroup levelOverGroup;

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
        float val = (float)currentLeaves / totalLeaves;
        val = Mathf.Min(1f, val);
        progressBar.value = val;
        progressText.text = $"<b>{val * 100:N0}%</b> Cleaned!";
    }

    public async UniTask LevelOverTransitionIn()
    {
        float time = 0f;
        while (time <= 1f)
        {
            time += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update);
            levelOverGroup.alpha = 1 - time;
        }
    }
    
    public async UniTask LevelOverTransitionOut()
    {
        float time = 0f;
        while (time <= 1f)
        {
            time += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update);
            levelOverGroup.alpha = (time);
        }
    }
    
}