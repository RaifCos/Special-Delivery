using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Script to handle all the obstacles on stage.
public class ObstacleManager : MonoBehaviour {
    private static WaitForSeconds _waitForSeconds0_02 = new WaitForSeconds(0.02f);
    private static WaitForSeconds _waitForSeconds8 = new WaitForSeconds(8f);
    public GameObject[] carStartingNodes, ufoStartingNodes, edgeNodesA, edgeNodesB;
    private Obstacle[] obstacles;
    private GameObject obstacleObject, destroyParticles;
    private readonly List<GameObject> currentObstacles = new();
    private int totalLimit, totalPermCount;

    void Awake() {
        GameManager.obstacleManager = this;
    }
    
    private void Start() {
        destroyParticles = Instantiate(Resources.Load<GameObject>("DestroyedParticle"));
        obstacles = GameManager.obstacleData.GetObstacles();
        // Dynamically retrieve the total number of possible Permanent obstacles allowed at once. 
        foreach (var obstacle in obstacles) { if (obstacle.isPermanent) { totalLimit += obstacle.limit; } }
    }

    // Function to spawn the starting obstacles at the start of the game.
    public void SpawnStartingObstacles() {
        // Reset Object Counts (from Previous Games)
        totalPermCount = 3;
        foreach (var obstacle in obstacles) { obstacle.SetCount(0); }
        // Spawn three random Cars (Red or Blue).
        for (int i = 0; i < 3; i++) {
            int gen = Random.Range(0, 2);
            obstacleObject = Instantiate(Resources.Load<GameObject>("Obstacles/"+obstacles[gen].intenalName));
            AddObstacle(obstacleObject);
            obstacles[gen].IncrementCount();
            GameManager.obstacleData.AddObstacleEncounter(gen);
        }
    }

    // Function to generate set a starting position for an obstacle 
    public GameObject GetStartingNode(int type) {
        Vector3 playerPosition = GameObject.Find("Player").transform.position;
        GameObject startingNode;
        if (type == 0) { // This Obstacle uses the Traffic Node set.
            do { // While Loops make sure the obstacle doesn't spawn in on top of the player.
                startingNode = carStartingNodes[Random.Range(0, carStartingNodes.Length)];
            } while (Vector3.Distance(startingNode.transform.position, playerPosition) < 10f);
        } else { // This Obstacle uses the UFO Node set.
            do {
                startingNode = carStartingNodes[Random.Range(0, carStartingNodes.Length)];
            } while (Vector3.Distance(startingNode.transform.position, playerPosition) < 10f);
         } return startingNode;
    }

    // Function to generate a Path that goes from one edge of the Stage to the Other.
    public Vector3[] GetEdgePath() {
        Vector3[] res = new Vector3[2];
        int route = Mathf.RoundToInt(Random.Range(0, edgeNodesA.Length));
        if (Random.Range(0, 2) == 0) {
            res[0] = edgeNodesA[route].transform.position;
            res[1] = edgeNodesB[route].transform.position;
        } else {
            res[0] = edgeNodesB[route].transform.position;
            res[1] = edgeNodesA[route].transform.position;
        }
        return res;
    }

    // Function to generate an Edge Path starting at the point closest to the player. 
    public Vector3[] GetClosestEdgePath() {
        Vector3[] res = new Vector3[2];
        Vector3 playerPosition = GameObject.Find("Player").transform.position;
        Vector3 nearestPoint = edgeNodesA[0].transform.position;
        int bestRoute = 0;

        for (int i=0; i<edgeNodesA.Length; i++) {
            if (Vector3.Distance(playerPosition, edgeNodesA[i].transform.position) < Vector3.Distance(playerPosition, nearestPoint)) {
                nearestPoint = edgeNodesA[i].transform.position;
                bestRoute = i;
            }
        }
        
        res[0] = edgeNodesA[bestRoute].transform.position;
        res[1] = edgeNodesB[bestRoute].transform.position;

        return res;
    }

    // Function to add an object to the list of current obstacles on stage.
    public void AddObstacle(GameObject obstacle) { currentObstacles.Add(obstacle); }

    public void SpawnObstacle(bool perm) {
        int gen;
        if (perm && totalPermCount <= totalLimit) {
            do { gen = Random.Range(0, obstacles.Length); } while (!obstacles[gen].isPermanent || !obstacles[gen].CheckLimit());
            totalPermCount++;
        } else { 
            do { gen = Random.Range(0, obstacles.Length); } while (obstacles[gen].isPermanent);
        }
        obstacleObject = Instantiate(Resources.Load<GameObject>("Obstacles/"+obstacles[gen].intenalName));
        AddObstacle(obstacleObject);
        GameManager.obstacleData.AddObstacleEncounter(gen);
        GameManager.newsTextScroller.newsQueue.Add(obstacles[gen].headline);
        obstacles[gen].IncrementCount();
    }

    // Coroutine to handle the removal of an obstacle from the game during gameplay.
    public IEnumerator ShrinkAndDestroy(GameObject obj, bool isObstacle) {
        if (!isObstacle) { yield return _waitForSeconds8; }
        destroyParticles.transform.position = obj.transform.position;
        destroyParticles.GetComponent<ParticleSystem>().Play();
        Vector3 scale = obj.transform.localScale;
        while (Mathf.Min(scale.x, scale.y, scale.z) > 0.1f) {
            // Rapidly Shrink the Object Slightly 
            obj.transform.localScale = obj.transform.localScale - new Vector3(0.05f, 0.05f, 0.05f);
            yield return _waitForSeconds0_02;
        } // Object has Shrunk to near-invisibility, so now Destroy.
        Destroy(obj);
        if (isObstacle) { currentObstacles.Remove(obj); }
    }
}
