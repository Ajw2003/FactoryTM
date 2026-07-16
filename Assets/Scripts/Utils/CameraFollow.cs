using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10f);

    private void LateUpdate()
    {
        if (target == null)
        {
            if (PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }
            else return;
        }

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.unscaledDeltaTime);
        transform.position = smoothedPosition;
    }
}
