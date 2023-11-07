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
        ResetLevelOverUI();
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

    private void ResetLevelOverUI()
    {
        levelOverGroup.alpha = 0f;
    }

    public async UniTask LevelOverTransitionIn()
    {
        var token = gameObject.GetCancellationTokenOnDestroy();
        try
        {
            float time = 0f;
            while (time <= 1f)
            {
                time += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
                token.ThrowIfCancellationRequested();
                levelOverGroup.alpha = 1 - time;
            }
        }
        finally
        {
            ResetLevelOverUI();
        }
    }
    
    public async UniTask LevelOverTransitionOut()
    {
        var token = gameObject.GetCancellationTokenOnDestroy();
        try
        {
            float time = 0f;
            while (time <= 1f)
            {
                time += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
                token.ThrowIfCancellationRequested();
                levelOverGroup.alpha = time;
            }
        }
        finally
        {
            ResetLevelOverUI();
        }
    }
    
}