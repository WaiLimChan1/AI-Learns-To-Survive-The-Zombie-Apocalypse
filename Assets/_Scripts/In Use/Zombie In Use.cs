using UnityEngine;

public class ZombieInUse : MonoBehaviour
{
    [Header("Zombie Components")]
    private Rigidbody rb;

    [Header("Zombie Stats")]
    private float movementSpeed = 1.0f;
    private float rotationSpeed = 10.0f;
    private float detectionRadius = 100f;

    [Header("Target")]
    [SerializeField] private SurvivorInUse targetSurvivor;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        //Target the closest survivor
        Collider[] detectedObjects = Physics.OverlapSphere(transform.position, detectionRadius, LayerMask.GetMask("Survivor"));
        if (detectedObjects != null && detectedObjects.Length > 0)
        {
            float closestDistance = Mathf.Infinity;
            SurvivorInUse closestSurvivor = null;

            foreach (Collider col in detectedObjects)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSurvivor = col.gameObject.GetComponent<SurvivorInUse>();
                }
            }

            targetSurvivor = closestSurvivor;
        }
        else
        {
            targetSurvivor = null;
        }

        if (targetSurvivor == null) return;
        Vector3 direction = (targetSurvivor.gameObject.transform.localPosition - transform.localPosition).normalized;
        Vector3 changeVector = direction * movementSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + changeVector);

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothRotation);
    }
}
