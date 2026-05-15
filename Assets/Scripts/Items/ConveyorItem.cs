using UnityEngine;

public enum ResourceType
{
    Copper,
    Ston,
    Iron,
    Diamond,
    Coal,
    Titanium,
    Uranium,
    Quartz
    
}

public class ConveyorItem : MonoBehaviour
{
    private Vector3 targetPosition;
    private Vector3Int currentCell;
    public float moveSpeed = 2f;
    public float value = 10f;
    public ResourceType resourceType;

    public bool IsMoving { get; private set; }
    private bool isInitialized = false;

    public void Initialize(Vector3Int startCell)
    {
        currentCell = startCell;
        ItemTracker.Instance.RegisterItem(this, currentCell);
        isInitialized = true;
    }

    private void Start()
    {
        if (!isInitialized)
        {
            currentCell = GameManager.Instance.buildingTilemap.WorldToCell(transform.position);
            ItemTracker.Instance.RegisterItem(this, currentCell);
            isInitialized = true;
        }
    }

    private void Update()
    {
        if (IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                transform.position = targetPosition;
                IsMoving = false;
            }
        }
    }

    public void SetTarget(Vector3Int targetCell, float speed)
    {
        if (IsMoving) return;

        Vector3Int oldCell = currentCell;
        currentCell = targetCell;
        ItemTracker.Instance.UpdateItemCell(this, oldCell, currentCell);

        targetPosition = GameManager.Instance.buildingTilemap.GetCellCenterWorld(targetCell);
        moveSpeed = speed;
        IsMoving = true;
    }

    private void OnDestroy()
    {
        if (ItemTracker.Instance != null && isInitialized)
        {
            ItemTracker.Instance.UnregisterItem(this, currentCell);
        }
    }
}
