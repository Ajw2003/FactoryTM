using System.Collections.Generic;
using Singleton;
using UnityEngine;

public class BuildingManager : SingletonBase<BuildingManager>
{
    private List<BuildingLogic> buildings = new List<BuildingLogic>();

    public List<BuildingLogic> GetAllBuildings() => buildings;

    public void RegisterBuilding(BuildingLogic building)
    {
        buildings.Add(building);
    }

    public void UnregisterBuilding(BuildingLogic building)
    {
        buildings.Remove(building);
    }

    private void Update()
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            buildings[i].PerformAction();
        }
    }
}
