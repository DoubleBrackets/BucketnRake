using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class LeafManager : MonoBehaviour
{
    private List<LeafSpawnGroup> leafGroups = new();

    [ShowInInspector]
    public int CleanedLeaves { set; get; }

    [ShowInInspector]
    public int TotalLeaves { set; get; }

    public event Action<int, int> OnLeavesChanged;

    public void InitializeLeaves()
    {
        // Find the leaves
        var leaves = FindObjectsOfType<LeafSpawnGroup>().Where(a => a.isActiveAndEnabled);
        leafGroups = leaves.ToList();

        // Instantiate leaves
        foreach (var leafGroup in leafGroups)
        {
            leafGroup.Initialize();
        }
    }

    public void RespawnLeaves()
    {
        foreach (var leafGroup in leafGroups)
        {
            leafGroup.ResetLeaves();
        }

        UpdateLeaveCount();
    }

    private void Update()
    {
        UpdateLeaveCount();
    }

    private void UpdateLeaveCount()
    {
        // Yes I know we should use events no I'm too lazy it's 2 am damnit
        var newCleanedLeaves = 0;
        var newTotalLeaves = 0;
        foreach (var leafGroup in leafGroups)
        {
            newCleanedLeaves += leafGroup.CleanedLeaves;
            newTotalLeaves += leafGroup.TotalLeaves;
        }

        var changed = false;
        if (newCleanedLeaves != CleanedLeaves)
        {
            CleanedLeaves = newCleanedLeaves;
            changed = true;
        }

        if (newTotalLeaves != TotalLeaves)
        {
            TotalLeaves = newTotalLeaves;
            changed = true;
        }

        if (changed)
        {
            OnLeavesChanged?.Invoke(CleanedLeaves, (int)(TotalLeaves * 0.975f));
        }
    }
}