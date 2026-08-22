using UnityEngine;
using System.Collections.Generic;

public class Pathway {
    readonly TrafficNode nextNode;
    readonly float distance;

    public Pathway(TrafficNode nN, float d) {
        nextNode = nN;
        distance = d;
    }

    public TrafficNode GetNextNode() => nextNode;
    public float GetDistance() => distance;
    
}

public class NodeGraph {
    private Dictionary<TrafficNode, Dictionary<TrafficNode, float>> distances = new();

    public void Build(TrafficNode[] nodeSet) { foreach (var n in nodeSet) distances[n] = Dijkstra(n, nodeSet); }

    private Dictionary<TrafficNode, float> Dijkstra(TrafficNode start, TrafficNode[] nodeSet) {
        var dist = new Dictionary<TrafficNode, float>();
        var visited = new HashSet<TrafficNode>();
        foreach (var n in nodeSet) dist[n] = Mathf.Infinity;
        dist[start] = 0f;

        var traversal = new List<TrafficNode> { start };
        while (traversal.Count > 0) {
            traversal.Sort((a, b) => dist[a].CompareTo(dist[b]));
            TrafficNode current = traversal[0];
            traversal.RemoveAt(0);
            if (!visited.Add(current)) continue;

            foreach (var pathway in current.GetPathways) {
                TrafficNode neighbor = pathway.GetNextNode();
                float newDist = dist[current] + pathway.GetDistance();
                if (newDist < dist[neighbor]) {
                    dist[neighbor] = newDist;
                    traversal.Add(neighbor);
                }
            }
        } return dist;
    }

    public float GetDistance(TrafficNode from, TrafficNode to) =>
        distances.TryGetValue(from, out var map) && map.TryGetValue(to, out float d) ? d : Mathf.Infinity;
}

public class TrafficNode : MonoBehaviour {    
    [SerializeField] private GameObject[] nextNodes;
    [SerializeField] private bool bossOnly = false;
    private readonly List<Pathway> pathways = new();
    private Vector3 pos;
    private int pathwayCount;

    void Awake() { pos = gameObject.transform.position; }

    void Start() {
        for (int i = 0; i < nextNodes.Length; i++) {
            if (nextNodes[i] != null) {
                pathways.Add(new(
                    nextNodes[i].GetComponent<TrafficNode>(),
                    Vector3.Distance(pos, nextNodes[i].transform.position)));
            }
        } pathwayCount = pathways.Count;
    }

    public Vector3 GetPos() => pos;

    public bool IsBossNode() => bossOnly;

    public TrafficNode GetNextNode() {
        if (pathwayCount == 1) return GetOnlyNextNode(); 
        return pathways[Random.Range(0, pathwayCount)].GetNextNode(); 
    }

    public TrafficNode GetNextNode(TrafficNode previousNode) {
        if (pathwayCount == 1) return GetOnlyNextNode(); 
        TrafficNode result;
        do { result = pathways[Random.Range(0, pathwayCount)].GetNextNode();
        } while (result == previousNode);
        return result;
    }

    private TrafficNode GetOnlyNextNode() => pathways[0].GetNextNode();

    public List<Pathway> GetPathways => pathways;
}