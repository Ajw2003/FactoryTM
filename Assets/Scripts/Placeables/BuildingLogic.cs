using UnityEngine;

public abstract class BuildingLogic : MonoBehaviour
{
    public Buildings.BuildingData data;
    protected Vector2Int myCell;
    public System.Collections.Generic.List<Vector2Int> occupiedCells = new System.Collections.Generic.List<Vector2Int>();

    public virtual void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
    {
        data = buildingData;
        myCell = cell;
    }

    public virtual void SetOccupiedCells(System.Collections.Generic.List<Vector2Int> cells)
    {
        occupiedCells = cells;
    }

    public Vector2Int GetMyCell()
    {
        return myCell;
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
