using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class GunInUse : Agent
{
    #region Gun Variables
    //----------------------------------------------------------------------------------------------------------------------------------------
    [Header("Bullet")]
    [SerializeField] private GameObject BulletInUsePrefab;

    [Header("Owner")]
    [SerializeField] private SurvivorInUse survivor;

    [Header("Gun Variables")]
    [SerializeField] public float angleYRotation;

    [Header("Gun Stats")]
    [SerializeField] public float rotationSpeed = 360f;

    [SerializeField] public Transform BulletSpawnPoint;
    [SerializeField] public float ShootCoolDown = 2.0f;
    [SerializeField] public float ShootCoolDownTimer;
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region ML Agent Functions
    //----------------------------------------------------------------------------------------------------------------------------------------
    public override void Initialize()
    {

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
                //Create Bullet
                GameObject bulletGO = Instantiate(BulletInUsePrefab);
                bulletGO.transform.position = BulletSpawnPoint.position;
                bulletGO.transform.eulerAngles = transform.eulerAngles;

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



    #region Update Functions
    //----------------------------------------------------------------------------------------------------------------------------------------
    private void FixedUpdate()
    {
        ShootCoolDownTimer -= Time.fixedDeltaTime;
        if (ShootCoolDownTimer < 0) ShootCoolDownTimer = 0;

        transform.localPosition = survivor.transform.localPosition;
    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion
}
