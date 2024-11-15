using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;

public class Survivor : Agent
{
    #region Survivor Variables
    //----------------------------------------------------------------------------------------------------------------------------------------
    //Events
    public event Action<string> OnSuccess;
    public event Action<string> OnFailure;

    [Header("Environment")]
    [SerializeField] private MLEnvironment MLEnvironment;

    [Header("Survivor Components")]
    private Rigidbody rb;

    [Header("Survivor Stats")]
    private float moveSpeed = 10;
    private float rotationSpeed = 150;

    [Header("Getting Food That Are Closer Together")]
    [SerializeField] public Vector3 lastFoodLocation;

    [Header("Same Location")]
    [SerializeField] public float SameLocationRadius;
    [SerializeField] public Vector3 lastRecordedSameLocation;
    [SerializeField] public float RecordSameLocationTime;
    [SerializeField] public float RecordSameLocationTimer;
    
    [Header("Rewards")]
    [SerializeField] public float CollectFoodReward;
    [SerializeField] public float CollectedAllFoodReward;

    [Header("Punishments")]
    [SerializeField] public float stayedInSameLocationPunishment;
    [SerializeField] public float collisionStayWithObstaclePunishment;
    [SerializeField] public float HitWallPunishment;
    [SerializeField] public float HitObstaclePunishment;
    [SerializeField] public float HitZombiePunishment;
    [SerializeField] public float RanOutOfTimePunishment;
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region ML Agent Functions
    //----------------------------------------------------------------------------------------------------------------------------------------
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = MLEnvironment.GetRandomEnvironmentPosition(MLEnvironment.SurvivorSpawnZone);
        MLEnvironment.SetUpMLEnvironmentOnEpisodeBegin();

        lastRecordedSameLocation = transform.localPosition;
        RecordSameLocationTimer = RecordSameLocationTime;

        lastFoodLocation = new Vector3(0,0,0);
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



    #region Rewards and Punishments
    //----------------------------------------------------------------------------------------------------------------------------------------
    public void OverloadedEndEpisode(Color resultColor, float reward, string message = "")
    {
        MLEnvironment.GroundRenderer.material.color = resultColor;

        AddReward(reward);

        Debug.Log(MLEnvironment.gameObject.name + " Survivor Reward: " + GetCumulativeReward() + " " + message);

        EndEpisode();
    }

    public void EpisodeFailed(string message, float punishment)
    {
        OverloadedEndEpisode(Color.red, punishment, message);
        OnFailure?.Invoke(message);
    }

    public void EpisodeSucceeded(string message, float reward)
    {
        OverloadedEndEpisode(Color.green, reward, message);
        OnSuccess?.Invoke(message);
    }

    private void RanOutOfTime()
    {
        if (MLEnvironment.RemainingTime <= 0) EpisodeFailed("Ran Out Of Time", RanOutOfTimePunishment);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall") EpisodeFailed("Hit Wall", HitWallPunishment);
        //else if (collision.gameObject.tag == "Obstacle") EpisodeFailed("Hit Obstacle", HitObstaclePunishment);
        else if (collision.gameObject.tag == "Zombie") EpisodeFailed("Hit Zombie", HitZombiePunishment);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Food")
        {
            //AddRewardForLastFoodDistance(collision.gameObject); //Temporary Punishment
            AddReward(CollectFoodReward);
            MLEnvironment.DestroyAndRemoveFood(collision.gameObject);

            if (MLEnvironment.SpawnedFoodList.Count == 0)
                EpisodeSucceeded("Got All Food", CollectedAllFoodReward + MLEnvironment.RemainingTime);
        }
    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region Temporary Rewards and Punishments
    //----------------------------------------------------------------------------------------------------------------------------------------
    private void AddRewardForLastFoodDistance(GameObject currentFood)
    {
        float distance = Vector3.Distance(currentFood.transform.localPosition, lastFoodLocation);
        lastFoodLocation = currentFood.transform.localPosition;

        AddReward(Mathf.Max(-distance, -CollectFoodReward));
    }

    private void SameLocationTimerLogic()
    {
        RecordSameLocationTimer -= Time.fixedDeltaTime;
        if (RecordSameLocationTimer <= 0)
        {
            if (Vector3.Distance(transform.localPosition, lastRecordedSameLocation) <= SameLocationRadius)
            {
                EpisodeFailed("Stuck In One Position", stayedInSameLocationPunishment);
            }
            lastRecordedSameLocation = transform.localPosition;
            RecordSameLocationTimer = RecordSameLocationTime;
        }
    }

    private void OnDrawGizmosSameLocation()
    {
        Gizmos.color = Color.black;

        Vector3 position = MLEnvironment.gameObject.transform.TransformPoint(lastRecordedSameLocation);
        position = new Vector3(position.x, 0, position.z);
        Gizmos.DrawWireSphere(position, SameLocationRadius);
    }

    private void OnCollisionStay(Collision collision)
    {
        //if (collision.gameObject.tag == "Obstacle") AddReward(collisionStayWithObstaclePunishment); //Temporary Punishment
    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    public void FixedUpdate()
    {
        RanOutOfTime();
        //SameLocationTimerLogic(); //Temporary Punishment

        MLEnvironment.SurvivorCumulativeReward = GetCumulativeReward();
    }

    private void OnDrawGizmos()
    {
        //OnDrawGizmosSameLocation();
    }
}
