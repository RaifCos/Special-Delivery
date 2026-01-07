using UnityEngine;

public class TrafficNode : MonoBehaviour {
    public GameObject[] nextNodes;

    public GameObject GetNextNode(GameObject prevNode) {
        GameObject resNode;
        do { resNode = nextNodes[Random.Range(0, nextNodes.Length)]; }
        while (resNode == prevNode);
        return resNode;
    }

    public GameObject GetNextClosestNode(GameObject prevNode, Vector3 target) {
        GameObject resNode = GetNextNode(prevNode);
        foreach (var possibleNode in nextNodes) {
            if (possibleNode != prevNode && Vector3.Distance(resNode.transform.position, target) > Vector3.Distance(possibleNode.transform.position, target))
            { resNode = possibleNode; }
        } return resNode;
    }
}
