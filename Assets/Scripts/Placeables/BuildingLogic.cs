using UnityEngine;

public abstract class BuildingLogic : MonoBehaviour
{
    public Buildings.BuildingData data;
    protected Vector2Int myCell;

    public virtual void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
    {
        data = buildingData;
        myCell = cell;
    }

    protected virtual void OnEnable()
    {
        if (BuildingManager.Instance != null)
            BuildingManager.Instance.RegisterBuilding(this);
    }

    protected virtual void OnDisable()
    {
        if (BuildingManager.Instance != null)
            BuildingManager.Instance.UnregisterBuilding(this);
    }

    public abstract void PerformAction();
}
