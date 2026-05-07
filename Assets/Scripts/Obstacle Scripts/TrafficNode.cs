using UnityEngine;
using System.Collections.Generic;

public class TrafficNode : MonoBehaviour {
    [SerializeField] private GameObject[] nextNodes;
    private TrafficNode[] nextTrafficNodes;

    void Start() {
        nextTrafficNodes = new TrafficNode[nextNodes.Length];
        for (int i = 0; i < nextNodes.Length; i++) {
            if (nextNodes[i] != null)
                nextTrafficNodes[i] = nextNodes[i].GetComponent<TrafficNode>();
        }
    }

    public GameObject GetNextNode(GameObject prevNode) {
        if (nextNodes == null || nextNodes.Length == 0) return null;
        if (nextNodes.Length == 1) return nextNodes[0];

        var candidates = new List<GameObject>();
        foreach (var node in nextNodes) { if (node != prevNode) candidates.Add(node); }
        
        if (candidates.Count == 0) return nextNodes[0];
        return candidates[Random.Range(0, candidates.Count)];
    }

    // Entry point called from CarTraversal - depth param kept for API compat but ignored
    public GameObject GetNextClosestNode(GameObject prevNode, Vector3 target, int depth) {
        if (nextNodes == null || nextNodes.Length == 0) return null;
        if (nextNodes.Length == 1) return nextNodes[0];

        // Find the node closest to the target as our goal
        TrafficNode goalNode = FindClosestNodeInGraph(target);
        if (goalNode == null) return GetNextNode(prevNode);

        // A* from this node to the goal, return the first step
        GameObject firstStep = AStarNextStep(prevNode, goalNode);
        return firstStep ?? GetNextNode(prevNode);
    }

    // Walks the full reachable graph to find whichever node is geographically
    // nearest to the target position - this becomes the A* destination
    private TrafficNode FindClosestNodeInGraph(Vector3 target) {
        TrafficNode best = null;
        float bestDist = float.MaxValue;

        var visited = new HashSet<TrafficNode>();
        var queue = new Queue<TrafficNode>();
        queue.Enqueue(this);
        visited.Add(this);

        while (queue.Count > 0) {
            TrafficNode current = queue.Dequeue();
            float dist = Vector3.Distance(current.transform.position, target);
            if (dist < bestDist) {
                bestDist = dist;
                best = current;
            }

            if (current.nextTrafficNodes == null) continue;
            foreach (var next in current.nextTrafficNodes) {
                if (next != null && !visited.Contains(next)) {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        return best;
    }

    // A* from this node to goal; returns the immediate next GameObject to move toward
    private GameObject AStarNextStep(GameObject prevNode, TrafficNode goal) {
        var openSet = new SortedList<float, (TrafficNode node, TrafficNode cameFromChild)>(new DuplicateKeyComparer());
        var cameFrom = new Dictionary<TrafficNode, TrafficNode>(); // child -> parent
        var gScore = new Dictionary<TrafficNode, float>();

        gScore[this] = 0f;
        float h = Vector3.Distance(transform.position, goal.transform.position);
        openSet.Add(h, (this, null));

        while (openSet.Count > 0) {
            var (current, _) = openSet.Values[0];
            openSet.RemoveAt(0);

            if (current == goal) {
                // Reconstruct path back to find the first step from `this`
                return ReconstructFirstStep(cameFrom, goal);
            }

            if (current.nextTrafficNodes == null) continue;
            for (int i = 0; i < current.nextTrafficNodes.Length; i++) {
                TrafficNode neighbor = current.nextTrafficNodes[i];
                if (neighbor == null) continue;

                // Don't reverse back through prevNode on the very first step
                if (current == this && current.nextNodes[i] == prevNode) continue;

                float tentativeG = gScore[current] +
                    Vector3.Distance(current.transform.position, neighbor.transform.position);

                if (!gScore.TryGetValue(neighbor, out float existingG) || tentativeG < existingG) {
                    gScore[neighbor] = tentativeG;
                    cameFrom[neighbor] = current;
                    float f = tentativeG + Vector3.Distance(neighbor.transform.position, goal.transform.position);
                    openSet.Add(f, (neighbor, current));
                }
            }
        }
        return null; // No path found
    }

    private GameObject ReconstructFirstStep(Dictionary<TrafficNode, TrafficNode> cameFrom, TrafficNode goal) {
        TrafficNode current = goal;
        TrafficNode previous = null;

        // Walk back until previous == this (the starting node)
        while (cameFrom.TryGetValue(current, out TrafficNode parent)) {
            if (parent == this) {
                // `current` is the first step - find its GameObject
                for (int i = 0; i < nextTrafficNodes.Length; i++) {
                    if (nextTrafficNodes[i] == current) return nextNodes[i];
                }
            }
            previous = current;
            current = parent;
        }
        return null;
    }

    // SortedList requires unique keys - this comparer breaks ties by returning 1
    private class DuplicateKeyComparer : IComparer<float> {
        public int Compare(float x, float y) {
            int result = x.CompareTo(y);
            return result == 0 ? 1 : result;
        }
    }
}