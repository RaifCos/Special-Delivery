using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// Script to handle all the obstacles on stage.
public class ObstacleManager : MonoBehaviour {

    [Header ("Obstacle Pools")]
    [SerializeField] private List<Obstacle> startingObstacles;
    [SerializeField] private List<Obstacle> permObstacles;
    [SerializeField] private List<Obstacle> tempObstacles;
    private readonly List<GameObject> permObstaclePool = new(); 
    private readonly List<GameObject> tempObstaclePool = new(); 

    [Header ("Obstacle Node Data")]
    [SerializeField] private float spawningDistanceThreshold;
    private TrafficNode[] trafficNodes, giantNodes;
    private GameObject[] sideNodes, edgeNodes;
    private bool[] sideNodeOccupied;

    private GameObject obstacleObject, destroyParticles;
    private int difficulty;
    private static readonly WaitForSeconds _waitForSeconds0_02 = new(0.02f);
    private static readonly WaitForSeconds _waitForSeconds8 = new(8f);

    void Awake() => GameManager.obstacleManager = this;
    
    private void Start() {
        // Load Node Data
        GameObject[] tnObj = GameObject.FindGameObjectsWithTag("Traffic Node");
        trafficNodes = tnObj.Select(o => o.GetComponent<TrafficNode>()).ToArray();

        GameObject[] gnObj = GameObject.FindGameObjectsWithTag("Giant Node");
        giantNodes = gnObj.Select(o => o.GetComponent<TrafficNode>()).ToArray();
        edgeNodes = gnObj.Where(go => go.GetComponent<EdgeNode>() != null).ToArray();

        sideNodes = GameObject.FindGameObjectsWithTag("Side Node");
        sideNodeOccupied = new bool[sideNodes.Length];
        for (int i = 0; i < sideNodes.Length; i++) { sideNodeOccupied[i] = false; }

        destroyParticles = Instantiate(Resources.Load<GameObject>("DestroyedParticle"));

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

        difficulty = GameManager.instance.GetDifficulty();
    }

    #region Node Functions

    // Function to generate set a starting position for an obstacle 
    public TrafficNode GetStartingNode(int type) {
        Vector3 playerPosition = GameManager.gameplayManager.FindPlayer();
        TrafficNode startingNode;
        if (type == 0) {
            do { startingNode = trafficNodes[Random.Range(0, trafficNodes.Length)];
            } while (Vector3.Distance(startingNode.GetPos(), playerPosition) < spawningDistanceThreshold);
        } else { 
            do { startingNode = giantNodes[Random.Range(0, giantNodes.Length)];
            } while (Vector3.Distance(startingNode.GetPos(), playerPosition) < spawningDistanceThreshold);
         } return startingNode;
    }

    public TrafficNode GetNearestNode(int type, float distanceThreshold) {
        Vector3 playerPosition = GameManager.gameplayManager.FindPlayer();
        TrafficNode nearestNode = type == 0? trafficNodes[0]: giantNodes[0];
        if (type == 0) {
            for (int i = 1; i < trafficNodes.Length; i++) {
                float distToTarget = Vector3.Distance(trafficNodes[i].transform.position, playerPosition);
                if (Vector3.Distance(nearestNode.GetPos(), playerPosition) >
                distToTarget && distToTarget > distanceThreshold) {
                    nearestNode = trafficNodes[i];
                }
            }
        } else {
            for (int i = 1; i < giantNodes.Length; i++) {
                float distToTarget = Vector3.Distance(giantNodes[i].GetPos(), playerPosition);
                if (Vector3.Distance(nearestNode.GetPos(), playerPosition) > 
                distToTarget && distToTarget > distanceThreshold) {
                    nearestNode = giantNodes[i];
                }
            }
        } return nearestNode;
    }

    // Function to generate a Path that goes from one edge of the Stage to the Other.
    public Vector3[] GetEdgePath() {
        GameObject edgeNode = edgeNodes[Mathf.RoundToInt(Random.Range(0, edgeNodes.Length))];
        return edgeNode.GetComponent<EdgeNode>().GetPath();
    }

    public Vector3[] GetClosestEdgePath(Vector3 target) {
        GameObject closestEdgeNode = edgeNodes[0];
        for (int i = 1; i < edgeNodes.Length; i++) {
            if (Vector3.Distance(target, edgeNodes[i].transform.position) < Vector3.Distance(target, closestEdgeNode.transform.position)) {
                closestEdgeNode = edgeNodes[i];
            }
        }
        
        return closestEdgeNode.GetComponent<EdgeNode>().GetPath();
    }

    // Function to find a node on the side of the road where a Magnet can spawn. 
    public Transform GetSideNode() {
        Transform res = null;
        int rand;
        do { rand = Mathf.RoundToInt(Random.Range(0, sideNodes.Length));
            if (!sideNodeOccupied[rand]) { res = sideNodes[rand].GetComponent<Transform>(); }
        } while (res == null);
        sideNodeOccupied[rand] = true;
        return res;
    }

    public TrafficNode[] GetNodeSet(int set) => set == 1? trafficNodes: giantNodes;

    #endregion

    #region Obstacle Spawning

    public void SpawnStartingObstacles() {
        foreach(Obstacle obs in startingObstacles) {
            obstacleObject = Instantiate(obs.so.prefab);
            obstacleObject.SetActive(true);
        }
    }

    public void SpawnObstacle(bool perm) {
        int gen;
        GameObject obstacleObject;

        if (perm && permObstaclePool.Count > 0) {
            gen = Random.Range(0, permObstaclePool.Count);
            obstacleObject = permObstaclePool[gen];
            permObstaclePool.Remove(obstacleObject);
        } else {
            do { gen = Random.Range(0, tempObstaclePool.Count);
                obstacleObject = tempObstaclePool[gen];
            } while (obstacleObject.activeInHierarchy);
        } 
        
        obstacleObject.SetActive(true);

        if (difficulty != 2) {
            Obstacle_SO obsSO = obstacleObject.GetComponent<Obstacle>().so;
            GameManager.newsTextScroller.newsQueue.Add(obsSO.headline);
        }
    }

    #endregion

    public IEnumerator ShrinkAndDestroy(GameObject obj, bool destroyObject, bool wait) {
        // Wait 8 Seconds (if specified)
        if(wait) yield return _waitForSeconds8; 

        // Play Destruction Particles
        destroyParticles.transform.position = obj.transform.position;
        destroyParticles.GetComponent<ParticleSystem>().Play();

        // Rapidly Shrink the Object Slightly 
        Vector3 scale = obj.transform.localScale;
        while (Mathf.Min(scale.x, scale.y, scale.z) > 0.1f) {
            obj.transform.localScale = obj.transform.localScale - new Vector3(0.05f, 0.05f, 0.05f);
            scale = obj.transform.localScale;
            yield return _waitForSeconds0_02;
        } // Destroy or Disable Object
        if (!destroyObject) { obj.SetActive(false); }
        else { Destroy(obj); }
    }
}
