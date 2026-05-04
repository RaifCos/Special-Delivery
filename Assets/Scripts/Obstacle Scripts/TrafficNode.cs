using UnityEngine;

public class TrafficNode : MonoBehaviour {
    [SerializeField] private GameObject[] nextNodes;

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

    public GameObject GetNextClosestNode(GameObject prevNode, Vector3 target, int depth) {
        GameObject bestNext = null;
        float bestDist = float.MaxValue;

        foreach (var candidate in nextNodes) {
            if (candidate == prevNode) continue;

            Vector3 lookaheadPos = depth > 0
                ? candidate.GetComponent<TrafficNode>()
                    .GetNextClosestNode(gameObject, target, depth - 1)
                    .transform.position
                : candidate.transform.position;

            float dist = Vector3.Distance(lookaheadPos, target);
            if (dist < bestDist) {
                bestDist = dist;
                bestNext = candidate;
            }
        }

        return bestNext;
    }
}
