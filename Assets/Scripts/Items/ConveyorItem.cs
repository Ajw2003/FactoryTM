using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using UnityEngine;


public class ConveyorItem : MonoBehaviour
{
    private Vector2 targetPosition;
    private Vector2Int currentCell;
    public float moveSpeed = 2f;
    public float value = 10f;
    public specificItemType itemType;
    public ItemData _itemData;

    public bool IsMoving { get; private set; }
    private bool isInitialized = false;

    public void Initialize(Vector2Int startCell)
    {
        currentCell = startCell;
        ItemTracker.Instance.RegisterItem(this, currentCell);
        isInitialized = true;
    }

    private void Start()
    {
        if (!isInitialized)
        {
            itemType = _itemData.specificItemType;
            var center = GridManager.Instance.center;
            currentCell = GridManager.Instance.WorldToCellConversion(center);
            transform.position = GridManager.Instance.CellToWorldConversion(currentCell);
            targetPosition = transform.position;
            ItemTracker.Instance.RegisterItem(this, currentCell);
            isInitialized = true;
        }
    }

    private void Update()
    {
        if (IsMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPosition) < 0.001f)
            {
                transform.position = targetPosition;
                IsMoving = false;
            }
        }
    }

    public void SetTarget(Vector2Int targetCell, float speed)
    {
        if (IsMoving) return;

        Vector2Int oldCell = currentCell;
        currentCell = targetCell;
        ItemTracker.Instance.UpdateItemCell(this, oldCell, currentCell);

        targetPosition = GridManager.Instance.CellToWorldConversion(currentCell);
        moveSpeed = speed;
        IsMoving = true;
    }

    private void OnDisable()
    {
        if (ItemTracker.HasInstance && isInitialized)
        {
            ItemTracker.Instance.UnregisterItem(this, currentCell);
            isInitialized = false;
        }
    }
}

