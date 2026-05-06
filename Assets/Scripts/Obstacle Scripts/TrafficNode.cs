using UnityEngine;

public class TrafficNode : MonoBehaviour {
    [SerializeField] private GameObject[] nextNodes;

    public GameObject GetNextNode(GameObject prevNode) {
        if (nextNodes == null || nextNodes.Length == 0) return null;

        if (nextNodes.Length == 1) return nextNodes[0];

        GameObject resNode;
        int attempts = 0;
        int maxAttempts = nextNodes.Length * 2;

        do {
            resNode = nextNodes[Random.Range(0, nextNodes.Length)];
            attempts++;
        } while (resNode == prevNode && attempts < maxAttempts);

        if (resNode == prevNode) {
            foreach (var candidate in nextNodes) {
                if (candidate != prevNode) {
                    resNode = candidate;
                    break;
                }
            }
        }

        return resNode;
    }

    public GameObject GetNextClosestNode(GameObject prevNode, Vector3 target) {
        GameObject resNode = GetNextNode(prevNode);
        if (resNode == null) return null;

        foreach (var possibleNode in nextNodes) {
            if (possibleNode != prevNode && Vector3.Distance(resNode.transform.position, target) > Vector3.Distance(possibleNode.transform.position, target)) {
                resNode = possibleNode;
            }
        } return resNode;
    }

    public GameObject GetNextClosestNode(GameObject prevNode, Vector3 target, int depth) {
        if (nextNodes == null || nextNodes.Length == 0)  return null;

        if (nextNodes.Length == 1) return nextNodes[0];

        GameObject bestNext = null;
        float bestDist = float.MaxValue;

        foreach (var candidate in nextNodes) {
            if (candidate == prevNode) continue;

            Vector3 lookaheadPos;
            if (depth > 0) {
                var nextNode = candidate.GetComponent<TrafficNode>()?.GetNextClosestNode(gameObject, target, depth - 1);
                lookaheadPos = nextNode != null ? nextNode.transform.position : candidate.transform.position;
            } else { lookaheadPos = candidate.transform.position; }

            float dist = Vector3.Distance(lookaheadPos, target);
            if (dist < bestDist) {
                bestDist = dist;
                bestNext = candidate;
            }
        }

        if (bestNext == null) return prevNode != null ? prevNode : nextNodes[0];
        return bestNext;
    }
}
