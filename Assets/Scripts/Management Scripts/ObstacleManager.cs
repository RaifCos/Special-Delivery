using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script to handle all the obstacles on stage.
public class ObstacleManager : MonoBehaviour {
    [SerializeField]
    private List<Obstacle> startingObstacles;
    [SerializeField]
    private List<Obstacle> permObstacles;
    [SerializeField]
    private List<Obstacle> tempObstacles;

    private readonly List<GameObject> permObstaclePool = new(); 
    private readonly List<GameObject> tempObstaclePool = new(); 
    private static readonly WaitForSeconds _waitForSeconds0_02 = new(0.02f);
    private static readonly WaitForSeconds _waitForSeconds8 = new(8f);
    public GameObject[] carStartingNodes, ufoStartingNodes, edgeNodesA, edgeNodesB, sideNodes;
    private GameObject obstacleObject, destroyParticles;
    private bool[] sideNodeOccupied; 

    void Awake() => GameManager.obstacleManager = this;
    
    private void Start() {
        // Reset Object Counts (from Previous Games)
        destroyParticles = Instantiate(Resources.Load<GameObject>("DestroyedParticle"));
        sideNodeOccupied = new bool[sideNodes.Length];
        for(int i=0; i<sideNodes.Length; i++) {
            sideNodeOccupied[i] = false;
        }

        // Add Perm Objects to Pool.
        foreach(Obstacle obs in permObstacles) { 
            obstacleObject = Instantiate(obs.so.prefab);
            permObstaclePool.Add(obstacleObject);
        }

        // Add Temp Objects to Pool.
        foreach(Obstacle obs in tempObstacles) { 
            obstacleObject = Instantiate(obs.so.prefab);
            tempObstaclePool.Add(obstacleObject);
        }
    }

    #region Node Functions

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
                startingNode = ufoStartingNodes[Random.Range(0, ufoStartingNodes.Length)];
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

    // Function to find a node on the side of the road where a Magnet can spawn. 
    public Transform GetSideNode() {
        Transform res = null;
        do {
            int rand = Mathf.RoundToInt(Random.Range(0, sideNodes.Length));
            if( !sideNodeOccupied[rand] ) { res = sideNodes[rand].GetComponent<Transform>(); }
        } while (res == null);
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

    #endregion

    #region Obstacle Spawning

    // Function to spawn the starting Obstacles at the beginning of the game.
    public void SpawnStartingObstacles() {
        foreach(Obstacle obs in startingObstacles) {
            obstacleObject = Instantiate(obs.so.prefab);
            permObstaclePool.Add(obstacleObject);
            obstacleObject.SetActive(true);
        }
    }

    // Function to spawn a Temporary or Permanent Obstacle.
    public void SpawnObstacle(bool perm) {
        int gen;
        GameObject obstacleObject;

        if (perm && permObstaclePool.Count > 0) {
            gen = Random.Range(0, permObstaclePool.Count);
            obstacleObject = permObstaclePool[gen];
            permObstaclePool.Remove(obstacleObject);
        } else {
            do {
                gen = Random.Range(0, tempObstaclePool.Count);
                obstacleObject = tempObstaclePool[gen];
            } while (obstacleObject.activeInHierarchy);
        } 
        
        obstacleObject.SetActive(true);
        Obstacle_SO obsSO = obstacleObject.GetComponent<Obstacle>().so;
        GameManager.newsTextScroller.newsQueue.Add(obsSO.headline);
    }

    #endregion

    // Coroutine to handle the removal of an obstacle from the game during gameplay.
    public IEnumerator ShrinkAndDestroy(GameObject obj, bool destroyObject) {
        yield return _waitForSeconds8;
        destroyParticles.transform.position = obj.transform.position;
        destroyParticles.GetComponent<ParticleSystem>().Play();
        Vector3 scale = obj.transform.localScale;
        while (Mathf.Min(scale.x, scale.y, scale.z) > 0.1f) {
            // Rapidly Shrink the Object Slightly 
            obj.transform.localScale = obj.transform.localScale - new Vector3(0.05f, 0.05f, 0.05f);
            scale = obj.transform.localScale;
            yield return _waitForSeconds0_02;
        } // Object has Shrunk to near-invisibility, so now Destroy.
        if (!destroyObject) { obj.SetActive(false); }
        else { Destroy(obj); }
    }
}
