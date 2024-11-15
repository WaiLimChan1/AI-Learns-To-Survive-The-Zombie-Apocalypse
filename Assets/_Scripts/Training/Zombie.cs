using System.Diagnostics.Contracts;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    [Header("Environment")]
    private MLEnvironment MLEnvironment;

    [Header("Zombie Components")]
    private Rigidbody rb;

    [Header("Zombie Stats")]
    private float movementSpeed = 1.0f;
    private float rotationSpeed = 100.0f;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetUp(MLEnvironment MLEnvironment)
    {
        this.MLEnvironment = MLEnvironment;
    }

    public void FixedUpdate()
    {
        if (MLEnvironment.Survivor == null) return;
        Vector3 direction = (MLEnvironment.Survivor.gameObject.transform.localPosition - transform.localPosition).normalized;
        Vector3 changeVector = direction * movementSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + changeVector);

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothRotation);
    }
}
