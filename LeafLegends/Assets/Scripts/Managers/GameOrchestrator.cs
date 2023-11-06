using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class GameOrchestrator : MonoBehaviour
{
    [SerializeField]
    private LeafManager leafManager;

    [SerializeField]
    private PlayerSpawner playerSpawner;

    [SerializeField]
    private SceneLoader sceneLoader;

    [SerializeField]
    private InGameUI inGameUI;

    [SerializeField]
    private List<string> levelSceneNames;

    [SerializeField]
    private int startLevelDebug;

    private int currentLevel = 0;

    private bool isLoading = false;

    private void Start()
    {
#if UNITY_EDITOR
        currentLevel = startLevelDebug;
#endif
        leafManager.OnLeavesChanged += CheckIfLevelCompleted;
        StartLevel(currentLevel);
    }

    private void CheckIfLevelCompleted(int cleaned, int total)
    {
        if (cleaned >= total)
        {
            OnLevelCompleted();
        }
    }

    [Button("Beat Leve")]
    public void BeatLevel()
    {
        OnLevelCompleted();
    }

    public async UniTaskVoid OnLevelCompleted()
    {
        if (isLoading)
            return;
        
        isLoading = true;
        currentLevel++;
        if (currentLevel >= levelSceneNames.Count)
        {
            Debug.Log("Game Completed!");
            return;
        }

        await UniTask.WaitForSeconds(2.5f);
        await inGameUI.LevelOverTransitionOut();
        await StartLevel(currentLevel);
        await inGameUI.LevelOverTransitionIn();

        isLoading = false;
    }

    private async UniTask StartLevel(int level)
    {
        await sceneLoader.LoadLevel(levelSceneNames[level]);
        leafManager.InitializeLeaves();
        leafManager.RespawnLeaves();
        playerSpawner.FindSpawns();
        playerSpawner.SpawnPlayers();
    }
}