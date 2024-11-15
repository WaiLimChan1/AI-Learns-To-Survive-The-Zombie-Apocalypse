using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentInUse : MonoBehaviour
{
    #region Environment In Use Variables
    //----------------------------------------------------------------------------------------------------------------------------------------
    [Header("Spawn Zones")]
    [SerializeField] public Transform SurvivorSpawnZone;
    [SerializeField] public Transform ObstacleSpawnZone;
    [SerializeField] public Transform ZombieSpawnZone;
    [SerializeField] public Transform FoodSpawnZone;

    [Header("Spawn Information")]
    [SerializeField] public float SurvivorAvoidDistance;
    [SerializeField] public float ObstacleAvoidDistance;
    [SerializeField] public float ZombieAvoidDistance;
    [SerializeField] public float FoodAvoidDistance;
    [SerializeField] public float SpawnDistanceBuffer = 1.0f;

    [Header("AgentInUse Information")]
    [SerializeField] private GameObject AgentInUsePrefab;
    [SerializeField] private Transform AgentInUseAnchor;
    [SerializeField] public List<GameObject> SpawnedSurvivorInUseList;
    [SerializeField] private int InitialAgentInUseNum;

    [Header("Obstacle Information")]
    [SerializeField] private GameObject ObstaclePrefab;
    [SerializeField] private Transform ObstacleAnchor;
    [SerializeField] public List<GameObject> SpawnedObstacleList;
    [SerializeField] private Vector2 ObstacleNumRange;

    [Header("ZombieInUse Information")]
    [SerializeField] private GameObject ZombiePrefab;
    [SerializeField] private Transform ZombieAnchor;
    [SerializeField] public List<GameObject> SpawnedZombieList;
    [SerializeField] private Vector2 ZombieNumRange;

    [Header("Food Information")]
    [SerializeField] private GameObject FoodPrefab;
    [SerializeField] private Transform FoodAnchor;
    [SerializeField] private int InitialFoodNum;
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    public void Awake()
    {
        SetAvoidDistances();
        SetUpEnvironment();
    }


    #region Spawn Position Helper Functions
    //----------------------------------------------------------------------------------------------------------------------------------------
    public void SetAvoidDistances()
    {
        SurvivorAvoidDistance = 1;

        GameObject Obstacle = Instantiate(ObstaclePrefab);
        ObstacleAvoidDistance = Obstacle.gameObject.GetComponent<BoxCollider>().bounds.size.x;
        Destroy(Obstacle);

        GameObject Zombie = Instantiate(ZombiePrefab);
        ZombieAvoidDistance = Zombie.gameObject.GetComponent<BoxCollider>().bounds.extents.x;
        Destroy(Zombie);

        GameObject food = Instantiate(FoodPrefab);
        FoodAvoidDistance = food.gameObject.GetComponent<SphereCollider>().bounds.extents.x;
        Destroy(food);
    }

    public Vector3 GetRandomEnvironmentPosition(Transform spawnZone)
    {
        Vector3 bounds = spawnZone.localScale;
        float width = Mathf.Abs(bounds.x) / 2;
        float length = Mathf.Abs(bounds.z) / 2;
        float height = 1;
        return new Vector3(Random.Range(-width, width), height, Random.Range(-length, length));
    }

    public Vector3 GetNonOverlappingPositionWithTarget(Transform spawnZone, Vector3 target, float minDistance)
    {
        Vector3 spawnPosition = GetRandomEnvironmentPosition(spawnZone);

        int maxIteration = 10;
        int counter = 0;
        while (Vector3.Distance(spawnPosition, target) <= minDistance && counter < maxIteration)
        {
            spawnPosition = GetRandomEnvironmentPosition(spawnZone);
            counter++;
        }

        return spawnPosition;
    }

    public bool OverlappingWithGameObjectList(List<GameObject> list, Vector3 checkPosition, float minDistance)
    {
        foreach (var i in list)
        {
            if (Vector3.Distance(checkPosition, i.transform.localPosition) <= minDistance)
                return true;
        }
        return false;
    }

    public Vector3 GetNonOverlappingPositionWithGameObjectList(List<GameObject> list, Transform spawnZone, float minDistance)
    {
        Vector3 spawnPosition = GetRandomEnvironmentPosition(spawnZone);

        int maxIteration = 10;
        int counter = 0;
        while (OverlappingWithGameObjectList(list, spawnPosition, minDistance) && counter < maxIteration)
        {
            spawnPosition = GetRandomEnvironmentPosition(spawnZone);
            counter++;
        }

        return spawnPosition;
    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region Spawn Agent Functions
    //----------------------------------------------------------------------------------------------------------------------------------------
    public void SpawnAgent()
    {
        GameObject Agent = Instantiate(AgentInUsePrefab);
        Agent.transform.parent = AgentInUseAnchor;
        //Agent.transform.GetChild(0).localPosition = GetNonOverlappingPositionWithGameObjectList(SpawnedSurvivorInUseList, SurvivorSpawnZone, SurvivorAvoidDistance + SurvivorAvoidDistance + SpawnDistanceBuffer);
        Agent.transform.GetChild(0).localPosition = new Vector3(10, 10, 10);
        SpawnedSurvivorInUseList.Add(Agent.transform.GetChild(0).gameObject);
    }

    public void SetUpSpawnedAgentInUseListOnEpisodeBegin()
    {
        Utils.DestroyAndRemoveAllFromList(SpawnedSurvivorInUseList);
        for (int i = 0; i < InitialAgentInUseNum; i++) SpawnAgent();
    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region Spawn Obstacle Functions
    //----------------------------------------------------------------------------------------------------------------------------------------
    public void SpawnObstacle()
    {
        GameObject Obstacle = Instantiate(ObstaclePrefab);
        Obstacle.transform.parent = ObstacleAnchor;
        Obstacle.transform.localPosition = GetNonOverlappingPositionWithGameObjectList(SpawnedSurvivorInUseList, ObstacleSpawnZone, SurvivorAvoidDistance + ObstacleAvoidDistance + SpawnDistanceBuffer);
        SpawnedObstacleList.Add(Obstacle);
    }

    public void SetUpSpawnedObstacleListOnEpisodeBegin()
    {
        Utils.DestroyAndRemoveAllFromList(SpawnedObstacleList);
        int spawnNum = Random.Range((int)ObstacleNumRange.x, (int)ObstacleNumRange.y + 1);
        for (int i = 0; i < spawnNum; i++) SpawnObstacle();
    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion


    #region Spawn Zombie Functions
    //----------------------------------------------------------------------------------------------------------------------------------------
    public void DestroyAndRemoveZombie(GameObject Zombie)
    {
        Utils.DetroyAndRemoveFromList(SpawnedZombieList, Zombie);
    }

    public void SpawnZombie()
    {
        GameObject Zombie = Instantiate(ZombiePrefab);
        Zombie.transform.parent = ZombieAnchor;
        Zombie.transform.localPosition = GetNonOverlappingPositionWithGameObjectList(SpawnedSurvivorInUseList, ZombieSpawnZone, SurvivorAvoidDistance + ZombieAvoidDistance + SpawnDistanceBuffer);
        SpawnedZombieList.Add(Zombie);
    }

    public void SetUpSpawnedZombieListOnEpisodeBegin()
    {
        Utils.DestroyAndRemoveAllFromList(SpawnedZombieList);
        int spawnNum = Random.Range((int)ZombieNumRange.x, (int)ZombieNumRange.y + 1);
        for (int i = 0; i < spawnNum; i++) SpawnZombie();
    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion


    #region Spawn Food Functions
    //----------------------------------------------------------------------------------------------------------------------------------------
    public void SpawnFood()
    {
        GameObject Food = Instantiate(FoodPrefab);
        Food.transform.parent = FoodAnchor;
        Food.transform.localPosition = GetNonOverlappingPositionWithGameObjectList(SpawnedObstacleList, FoodSpawnZone, FoodAvoidDistance + ObstacleAvoidDistance + SpawnDistanceBuffer);
    }

    public void SetUpSpawnedFoodListOnEpisodeBegin()
    {
        for (int i = 0; i < InitialFoodNum; i++) SpawnFood();
    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion



    #region Other
    //----------------------------------------------------------------------------------------------------------------------------------------
    public void SetUpEnvironment()
    {
        SetUpSpawnedAgentInUseListOnEpisodeBegin();
        SetUpSpawnedObstacleListOnEpisodeBegin();
        SetUpSpawnedZombieListOnEpisodeBegin();
        SetUpSpawnedFoodListOnEpisodeBegin();
    }
    //----------------------------------------------------------------------------------------------------------------------------------------
    #endregion
}
