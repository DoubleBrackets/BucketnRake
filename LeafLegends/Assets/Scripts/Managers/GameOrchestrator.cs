using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
    private List<string> levelSceneNames;

    [SerializeField]
    private int startLevelDebug;

    private int currentLevel = 0;

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
        if (cleaned == total)
        {
            OnLevelCompleted();
        }
    }

    public void OnLevelCompleted()
    {
        currentLevel++;
        if (currentLevel >= levelSceneNames.Count)
        {
            Debug.Log("Game Completed!");
            return;
        }

        StartLevel(currentLevel);
    }

    private async UniTaskVoid StartLevel(int level)
    {
        await sceneLoader.LoadLevel(levelSceneNames[level]);
        leafManager.InitializeLeaves();
        leafManager.RespawnLeaves();
        playerSpawner.FindSpawns();
        playerSpawner.SpawnPlayers();
    }
}