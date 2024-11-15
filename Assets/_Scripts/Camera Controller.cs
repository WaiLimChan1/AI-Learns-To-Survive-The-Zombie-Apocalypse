using UnityEngine;
public class CameraController : MonoBehaviour
{
    private float moveSpeed = 50f;

    private float zoomSpeed = 500f;
    private float minZoom = 10f;
    private float maxZoom = 50f;

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical) * moveSpeed * Time.deltaTime;
        transform.position = transform.position + movement;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            float newZoom = Mathf.Clamp(transform.position.y + zoomSpeed * scrollInput * Time.deltaTime, minZoom, maxZoom);
            transform.position = new Vector3(transform.position.x, newZoom, transform.position.z);
        }
    }
}