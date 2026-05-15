using UnityEngine;

public abstract class BuildingLogic : MonoBehaviour
{
    public Buildings.BuildingData data;
    protected Vector3Int myCell;

    public bool IsBeingWorked { get; set; } = false;
    public bool IsAssigned { get; set; } = false;
    public float EfficiencyMultiplier { get; set; } = 1f;

    public virtual void Setup(Buildings.BuildingData buildingData, Vector3Int cell)
    {
        data = buildingData;
        myCell = cell;
    }

    public Vector3Int GetCell() => myCell;

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
