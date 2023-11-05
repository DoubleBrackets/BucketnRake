using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject rakePlayerPrefab;
    
    [SerializeField]
    private GameObject bucketPlayerPrefab;
    
    [SerializeField]
    private PlayerAnchorSO playerAnchor;

    private GameObject rakePlayerInstance;
    private GameObject bucketPlayerInstance;
    
    [ShowInInspector, ReadOnly]
    private Transform rakeSpawnPos;
    
    [ShowInInspector, ReadOnly]
    private Transform bucketSpawnPos;

    private void Awake()
    {
        FindSpawns();
        SpawnPlayers();
    }

    private void OnValidate()
    {
        FindSpawns();
    }

    private void FindSpawns()
    {
        rakeSpawnPos = GameObject.Find("RakeSpawn").transform;
        bucketSpawnPos = GameObject.Find("BucketSpawn").transform;
    }

    public void SpawnPlayers()
    {
        if (rakePlayerInstance)
        {
            Destroy(rakePlayerInstance);
        }

        if (bucketPlayerInstance)
        {
            Destroy(bucketPlayerInstance);
        }
        
        rakePlayerInstance = Instantiate(rakePlayerPrefab, rakeSpawnPos.position, Quaternion.identity);
        bucketPlayerInstance = Instantiate(bucketPlayerPrefab, bucketSpawnPos.position, Quaternion.identity);

        playerAnchor.BucketPlayerController = bucketPlayerInstance.GetComponentInChildren<ProtagController>();
        playerAnchor.RakePlayerController = rakePlayerInstance.GetComponentInChildren<ProtagController>();
    }

    private void OnDrawGizmos()
    {
        if(rakeSpawnPos == null || bucketSpawnPos == null)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(rakeSpawnPos.position, 1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(bucketSpawnPos.position, 1f);
    }
}
