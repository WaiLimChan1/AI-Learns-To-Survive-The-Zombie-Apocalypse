using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Gun : Agent
{
    #region Gun Variables
    //----------------------------------------------------------------------------------------------------------------------------------------
    [Header("Environment")]
    [SerializeField] private MLEnvironment MLEnvironment;

    [Header("Gun Variables")]
    [SerializeField] public float angleYRotation;

    [Header("Gun Stats")]
    [SerializeField] public float rotationSpeed = 360f;

    [SerializeField] public Transform BulletSpawnPoint;
    [SerializeField] public float ShootCoolDown = 2.0f;
    [SerializeField] public float ShootCoolDownTimer;

    [Header("Reward And Punishment")]
    [SerializeField] public float SucceededReward;
    [SerializeField] public float FailedPunishment;

    [SerializeField] public float HitAllZombieReward;

    [SerializeField] public float HitZombieReward;
    [SerializeField] public float MissedZombiePunishment;
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region ML Agent Functions
    //----------------------------------------------------------------------------------------------------------------------------------------
    public override void Initialize()
    {
        MLEnvironment.Survivor.OnSuccess += HandleSurvivorSucceeded;
        MLEnvironment.Survivor.OnFailure += HandleSurvivorFailed;
    }

    public override void OnEpisodeBegin()
    {
        transform.localEulerAngles = Vector3.zero;
        angleYRotation = 0;
        ShootCoolDownTimer = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localEulerAngles.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;

        angleYRotation += (discreteActions[0] - 1) * rotationSpeed * Time.fixedDeltaTime;
        transform.localEulerAngles = new Vector3(0, angleYRotation, 0);

        if (discreteActions[1] == 1)
            if (ShootCoolDownTimer <= 0)
            {
                Bullet bullet = MLEnvironment.SpawnBullet(BulletSpawnPoint.position, transform.eulerAngles);
                bullet.OnBulletCollision += HandleBulletCollision;
                ShootCoolDownTimer = ShootCoolDown;
            }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        //Rotating Gun
        discreteActions[0] = 1;
        if (Input.GetKey(KeyCode.E)) discreteActions[0] += 1;
        if (Input.GetKey(KeyCode.Q)) discreteActions[0] -= 1;

        //Shooting Gun
        discreteActions[1] = 0;
        if (Input.GetKey(KeyCode.Space)) discreteActions[1] = 1;

    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region Rewards and Punishments
    //----------------------------------------------------------------------------------------------------------------------------------------
    public void OverloadedEndEpisode(float reward, string message = "")
    {
        MLEnvironment.Gun.AddReward(reward);

        Debug.Log(MLEnvironment.gameObject.name + " " + "Gun Reward: " + GetCumulativeReward() + " " + message);

        MLEnvironment.Gun.EndEpisode();
    }

    public void HandleSurvivorSucceeded(string message)
    {
        OverloadedEndEpisode(SucceededReward, message);
    }

    public void HandleSurvivorFailed(string message)
    {
        OverloadedEndEpisode(FailedPunishment, message);
    }

    private void HandleBulletCollision(GameObject collidedObject)
    {
        if (collidedObject.tag == "Wall")
        {
            MLEnvironment.Gun.AddReward(MLEnvironment.Gun.MissedZombiePunishment);
        }
        else if (collidedObject.tag == "Obstacle")
        {
            MLEnvironment.Gun.AddReward(MLEnvironment.Gun.MissedZombiePunishment);
        }
        else if (collidedObject.tag == "Zombie")
        {
            MLEnvironment.DestroyAndRemoveZombie(collidedObject);
            MLEnvironment.Gun.AddReward(MLEnvironment.Gun.HitZombieReward);

            if (MLEnvironment.SpawnedZombieList.Count == 0)
            {
                MLEnvironment.Gun.AddReward(MLEnvironment.Gun.HitAllZombieReward);
            }
        }
    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region Update Functions
    //----------------------------------------------------------------------------------------------------------------------------------------
    private void FixedUpdate()
    {
        ShootCoolDownTimer -= Time.fixedDeltaTime;
        if (ShootCoolDownTimer < 0) ShootCoolDownTimer = 0;

        MLEnvironment.GunCumulativeReward = GetCumulativeReward();
        transform.localPosition = MLEnvironment.Survivor.transform.localPosition;
    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion
}
