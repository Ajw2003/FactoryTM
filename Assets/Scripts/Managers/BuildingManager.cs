using System.Collections.Generic;
using Singleton;
using UnityEngine;

public class BuildingManager : SingletonBase<BuildingManager>
{
    private List<BuildingLogic> buildings = new List<BuildingLogic>();

    public void RegisterBuilding(BuildingLogic building)
    {
        buildings.Add(building);
    }

    public void UnregisterBuilding(BuildingLogic building)
    {
        buildings.Remove(building);
    }

    public int GetBuildingCount(Buildings.BuildingData data)
    {
        int count = 0;
        for (int i = 0; i < buildings.Count; i++)
        {
            if (buildings[i] != null && buildings[i].data == data)
            {
                count++;
            }
        }
        return count;
    }

    private void Update()
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            buildings[i].PerformAction();
        }
    }
}
