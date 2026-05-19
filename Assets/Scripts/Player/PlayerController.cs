using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    private Vector3 targetPosition;
    private Vector3 currentPos;
    private Vector3Int currentCell;
    [SerializeField] private float moveSpeed = 5f;

    // Update is called once per frame

    private void Start()
    {
        currentCell = GameManager.Instance.buildingTilemap.WorldToCell(transform.position);
        transform.position = GameManager.Instance.buildingTilemap.GetCellCenterWorld(currentCell);
        targetPosition = this.transform.position;
    }

    void Update()
    {
        Vector3Int inputDirection = Vector3Int.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputDirection = Vector3Int.up;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputDirection = Vector3Int.down;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputDirection = Vector3Int.left;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputDirection = Vector3Int.right;

        if (inputDirection != Vector3Int.zero)
        {
            Vector3Int nextCell = currentCell + inputDirection;
            SetTarget(nextCell, moveSpeed);
        }

        currentPos = transform.position;
        currentCell = GameManager.Instance.buildingTilemap.WorldToCell(currentPos);
        //set target to tile next to player in the direction of key press i.e. wasd up,down,lef,right
        //move from current cell to next cell based on input, get current cell +- 1 in direction presses
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            transform.position = targetPosition;
        }
        
    }
    
    public void SetTarget(Vector3Int targetCell, float speed)
    {

        Vector3Int oldCell = currentCell;
        currentCell = targetCell;

        targetPosition = GameManager.Instance.buildingTilemap.GetCellCenterWorld(targetCell);
    }
}
