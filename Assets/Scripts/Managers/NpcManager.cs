using System.Collections.Generic;
using Singleton;
using UnityEngine;

public class NpcManager : SingletonBase<NpcManager>
{
    public GameObject workerPrefab;
    public float hireCost = 500f;
    
    private List<WorkerNpc> workers = new List<WorkerNpc>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            HireWorker();
        }
    }

    public void HireWorker()
    {
        if (CurrencyManager.Instance.currentCurrencyValue >= hireCost)
        {
            CurrencyManager.Instance.RemoveCurrency(hireCost);
            SpawnWorker();
        }
        else
        {
            Debug.Log("Not enough currency to hire a worker!");
        }
    }

    private void SpawnWorker()
    {
        if (workerPrefab == null)
        {
            Debug.LogError("Worker prefab is not assigned in NpcManager!");
            return;
        }

        GameObject workerGO = Instantiate(workerPrefab, Vector3.zero, Quaternion.identity);
        WorkerNpc worker = workerGO.GetComponent<WorkerNpc>();
        if (worker != null)
        {
            workers.Add(worker);
            ScanAndAssignUnassignedBuildings(worker);
        }
    }

    private void ScanAndAssignUnassignedBuildings(WorkerNpc worker)
    {
        if (BuildingManager.Instance == null) return;

        var allBuildings = BuildingManager.Instance.GetAllBuildings();
        foreach (var building in allBuildings)
        {
            if (worker.assignedBuildings.Count >= worker.maxBuildings) break;

            if (!building.IsAssigned && !(building is ConveyorLogic)) // Belts don't need workers
            {
                worker.AssignBuilding(building);
                building.IsAssigned = true;
            }
        }
    }

    public void AssignBuildingToWorker(BuildingLogic building)
    {
        if (building is ConveyorLogic) return; // Belts don't need workers

        // Find a worker with the least amount of buildings and below their limit
        WorkerNpc bestWorker = null;
        int minBuildings = int.MaxValue;

        foreach (var worker in workers)
        {
            if (worker.assignedBuildings.Count < worker.maxBuildings)
            {
                if (worker.assignedBuildings.Count < minBuildings)
                {
                    minBuildings = worker.assignedBuildings.Count;
                    bestWorker = worker;
                }
            }
        }

        if (bestWorker != null)
        {
            bestWorker.AssignBuilding(building);
            building.IsAssigned = true;
        }
        else
        {
            Debug.LogWarning("No available workers for new building!");
            building.IsAssigned = false; // Stay unassigned
        }
    }
}
