using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script to handle all the obstacles on stage.
public class ObstacleManager : MonoBehaviour {
    private static WaitForSeconds _waitForSeconds0_02 = new(0.02f);
    private static WaitForSeconds _waitForSeconds8 = new(8f);
    public GameObject[] carStartingNodes, ufoStartingNodes, edgeNodesA, edgeNodesB;
    private List<Obstacle> obstacles;
    private readonly List<Obstacle> permObstacles = new();
    private readonly List<Obstacle> tempObstacles = new();
    private GameObject obstacleObject, destroyParticles;
    private readonly List<GameObject> currentObstacles = new();

    void Awake() {
        GameManager.obstacleManager = this;
    }
    
    private void Start() {
        destroyParticles = Instantiate(Resources.Load<GameObject>("DestroyedParticle"));
        obstacles = GameManager.obstacleData.GetObstacles();
        // Dynamically retrieve the total number of possible Permanent obstacles allowed at once. 
        Debug.Log(obstacles.Count);
        foreach (var obs in obstacles) { 
            if(obs.so.limit > 0) { permObstacles.Add(obs); }
            else { tempObstacles.Add(obs); }
        }
    }

    // Function to spawn the starting obstacles at the start of the game.
    public void SpawnStartingObstacles() {
        // Reset Object Counts (from Previous Games)
        GameManager.obstacleData.ResetGameEncounters();
        // Spawn three random Cars (Red or Blue).
        for (int i = 0; i < 3; i++) {
            string[] startingObs = {"carRed", "carBlue", "carGreen"};
            int random = Random.Range(0, 3);
            obstacleObject = Instantiate(Resources.Load<GameObject>("Obstacles/"+ startingObs[random]));
            AddObstacle(obstacleObject);
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
        Obstacle obs;
        Obstacle_SO obsSO;
        if (perm && permObstacles.Count > 0) {
            bool obstacleFound;
            do { gen = Random.Range(0, permObstacles.Count); 
                obs = permObstacles[gen];
                obstacleFound = !GameManager.obstacleData.CheckLimit(obs);
                if(!obstacleFound) { permObstacles.Remove(obs); }
            } while(!obstacleFound);
        } else {
            gen = Random.Range(0, tempObstacles.Count);
            obs = tempObstacles[gen];
        } obsSO = obs.so;
        GameObject obsObj = Instantiate(Resources.Load<GameObject>("Obstacles/"+obsSO.intenalName));
        GameManager.newsTextScroller.newsQueue.Add(obsSO.headline);
        AddObstacle(obsObj);
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
            scale = obj.transform.localScale;
            yield return _waitForSeconds0_02;
        } // Object has Shrunk to near-invisibility, so now Destroy.
        Destroy(obj);
        if (isObstacle) { currentObstacles.Remove(obj); }
    }
}
