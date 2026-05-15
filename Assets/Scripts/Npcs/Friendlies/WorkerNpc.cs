using System.Collections.Generic;
using UnityEngine;

public class WorkerNpc : NpcStateMachine
{
    public List<BuildingLogic> assignedBuildings = new List<BuildingLogic>();
    public int maxBuildings = 3;
    public float efficiency = 1.0f;
    public float workDuration = 2.0f;
    
    public int currentBuildingIndex = 0;

    void Start()
    {
        ChangeState(new WorkerIdleState(this));
    }

    public void AssignBuilding(BuildingLogic building)
    {
        if (assignedBuildings.Count < maxBuildings)
        {
            assignedBuildings.Add(building);
        }
    }
    
    public BuildingLogic GetCurrentTargetBuilding()
    {
        if (assignedBuildings.Count == 0) return null;
        if (currentBuildingIndex >= assignedBuildings.Count) currentBuildingIndex = 0;
        return assignedBuildings[currentBuildingIndex];
    }
    
    public void MoveToNextBuilding()
    {
        if (assignedBuildings.Count > 0)
        {
            currentBuildingIndex = (currentBuildingIndex + 1) % assignedBuildings.Count;
        }
    }
}

public class WorkerIdleState : NpcState
{
    private WorkerNpc _worker;
    public WorkerIdleState(WorkerNpc worker) : base(worker) { _worker = worker; }
    
    public override void Update()
    {
        if (_worker.assignedBuildings.Count > 0)
        {
            _worker.ChangeState(new WorkerMoveToBuildingState(_worker));
        }
    }
}

public class WorkerMoveToBuildingState : NpcState
{
    private WorkerNpc _worker;
    private BuildingLogic _target;
    
    public WorkerMoveToBuildingState(WorkerNpc worker) : base(worker) { _worker = worker; }
    
    public override void Enter()
    {
        _target = _worker.GetCurrentTargetBuilding();
    }
    
    public override void Update()
    {
        if (_target == null)
        {
            _worker.ChangeState(new WorkerIdleState(_worker));
            return;
        }
        
        Vector3 targetPos = GameManager.Instance.buildingTilemap.GetCellCenterWorld(_target.GetCell());
        _worker.transform.position = Vector3.MoveTowards(_worker.transform.position, targetPos, _worker.MoveSpeed * Time.deltaTime);
        
        if (Vector3.Distance(_worker.transform.position, targetPos) < 0.1f)
        {
            _worker.ChangeState(new WorkerWorkBuildingState(_worker, _target));
        }
    }
}

public class WorkerWorkBuildingState : NpcState
{
    private WorkerNpc _worker;
    private BuildingLogic _target;
    private float _timer;
    
    public WorkerWorkBuildingState(WorkerNpc worker, BuildingLogic target) : base(worker) 
    { 
        _worker = worker; 
        _target = target;
    }
    
    public override void Enter()
    {
        _timer = _worker.workDuration;
        if (_target != null)
        {
            _target.IsBeingWorked = true;
            _target.EfficiencyMultiplier = _worker.efficiency;
        }
    }
    
    public override void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0 || _target == null)
        {
            _worker.ChangeState(new WorkerIdleState(_worker)); // Transition back to decide next building
        }
    }
    
    public override void Exit()
    {
        if (_target != null)
        {
            _target.IsBeingWorked = false;
        }
        _worker.MoveToNextBuilding();
    }
}
