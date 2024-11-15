using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class SurvivorInUse : Agent
{
    #region Survivor Variables
    //----------------------------------------------------------------------------------------------------------------------------------------
    [Header("Survivor Components")]
    private Rigidbody rb;

    [Header("Survivor Stats")]
    private float moveSpeed = 10;
    private float rotationSpeed = 150;

    [Header("Invincible Timer")]
    private float invincibleTimer = 5;
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region ML Agent Functions
    //----------------------------------------------------------------------------------------------------------------------------------------
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteAction = actions.DiscreteActions;

        //Branch 0: Move forward or backward
        Vector3 changeVector = transform.forward * (discreteAction[0] - 1) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + changeVector);

        //Branch 1: Turn left or turn right
        float newRotationY = rb.rotation.eulerAngles.y + (discreteAction[1] - 1) * rotationSpeed * Time.fixedDeltaTime;
        Quaternion targetRotation = Quaternion.Euler(rb.rotation.eulerAngles.x, newRotationY, rb.rotation.eulerAngles.z);
        rb.MoveRotation(targetRotation);

        //Branch 2: No Jump or Jump
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteAction = actionsOut.DiscreteActions;

        //Branch 0: Move forward or backward
        discreteAction[0] = 1;
        if (Input.GetKey(KeyCode.W)) discreteAction[0] += 1;
        if (Input.GetKey(KeyCode.S)) discreteAction[0] -= 1;

        //Branch 1: Turn left or turn right
        discreteAction[1] = 1;
        if (Input.GetKey(KeyCode.A)) discreteAction[1] -= 1;
        if (Input.GetKey(KeyCode.D)) discreteAction[1] += 1;

        //Branch 2: No Jump or Jump

    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (invincibleTimer > 0) return;
        if (collision.gameObject.tag == "Zombie") Destroy(this.transform.parent.gameObject);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Food")
        {
            Destroy(collision.gameObject);
        }
    }

    private void FixedUpdate()
    {
        invincibleTimer -= Time.deltaTime;
    }
}
