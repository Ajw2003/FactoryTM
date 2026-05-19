using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector3 targetPosition;
    private Vector3Int currentCell;
    [SerializeField] private float moveSpeed = 5f;
    private bool isMoving = false;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.buildingTilemap != null)
        {
            currentCell = GameManager.Instance.buildingTilemap.WorldToCell(transform.position);
            transform.position = GameManager.Instance.buildingTilemap.GetCellCenterWorld(currentCell);
        }
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (isMoving) return;

        Vector3Int inputDirection = Vector3Int.zero;//set input to zero
        
        //get input and convert to direction in grid space +-1 in x or y coords

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputDirection.y += 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputDirection.y -= 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputDirection.x -= 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputDirection.x += 1;

        if (inputDirection != Vector3Int.zero)
        {
            Vector3Int nextCell = currentCell + inputDirection;//set the cell to move towards to your current cell plus the input direction
            StartCoroutine(MoveRoutine(nextCell));// start moving
        }
    }

    private IEnumerator MoveRoutine(Vector3Int targetCell)
    {
        isMoving = true;
        currentCell = targetCell;//change current cell to target
        
        if (GameManager.Instance != null && GameManager.Instance.buildingTilemap != null)
        {
            targetPosition = GameManager.Instance.buildingTilemap.GetCellCenterWorld(targetCell);// get position of target cell on the tile map
        }

        while (Vector3.Distance(transform.position, targetPosition) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);// move towards the target cell
            yield return null;
        }

        transform.position = targetPosition;// finalize and directly set position.
        isMoving = false;
    }

    public void SetTarget(Vector3Int targetCell)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveRoutine(targetCell));
        }
    }
}
