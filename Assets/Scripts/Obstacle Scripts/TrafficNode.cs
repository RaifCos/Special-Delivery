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

public class TrafficNode : MonoBehaviour {
    [SerializeField] private GameObject[] nextNodes;
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