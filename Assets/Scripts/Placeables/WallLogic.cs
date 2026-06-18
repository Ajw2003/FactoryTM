using UnityEngine;

public class WallLogic : BuildingLogic
{
    public override void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
    {
        base.Setup(buildingData, cell);
        Health = Mathf.RoundToInt(data.maxHealth * GetTierMultiplier());
    }

    public override void PerformAction()
    {
        // Walls don't perform actions, they just stand there and take damage.
    }
}
