using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarMovement))]
public class CarTraversal : MonoBehaviour {
    [Header("Traversal Variables")]
    [SerializeField] private float distanceThreshold;
    [SerializeField] private float height;

    [Header("Traversal Information")]
    [SerializeField] private int nodeSet;
    [SerializeField] private bool usesBossNodes;
    [SerializeField] private bool ignoreBlockage;
    [SerializeField] private TrafficNode startingNode;

    [Header("Target Following")]
    [SerializeField] private GameObject target;
    [SerializeField] private bool followPlayer;
    [SerializeField] private bool chaseTarget;
    [SerializeField] private float directChaseRange;
    [SerializeField] private float returnToNodeRange;

    private bool hasTarget; 
    private bool isChasing;

    private Rigidbody rb;
    private CarMovement cM;
    private TrafficNode currNode, prevNode;
    private NodeGraph graph;
    private LayerMask blockageMask, roadMask;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        cM = GetComponent<CarMovement>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        blockageMask = LayerMask.GetMask("Blockage");
        roadMask = LayerMask.GetMask("Road");
    }

    void Start() {
        graph = GameManager.obstacleManager.GetGraph(nodeSet);
        Initialize();
    }

    void FixedUpdate() {
        cM.DecreaseCollisionTimer();
        cM.DecreaseStunTimer();

        if (cM.IsStunned) return;

        Vector3 rayOrigin = transform.position + Vector3.up;
        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit roadHit, height, roadMask)) return;

        if (!ignoreBlockage && Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, 10f, blockageMask)) {
            cM.AdjustTopSpeed(hit.distance);
        } else { cM.AdjustTopSpeed(); }
        
        UpdateChaseState();

        if (isChasing) {
            if (target == null) return;
            cM.DriveToward(roadHit.normal, target.transform.position);
            return;
        }

        if (currNode == null) return;
        if (Vector3.Distance(rb.position, currNode.transform.position) > distanceThreshold) {
            cM.DriveToward(roadHit.normal, currNode.transform.position);
        } else { AdvanceToNextNode(); }
    }

    private void Initialize() {
        if (followPlayer) { target = GameManager.gameplayManager.GetPlayer(); }
        hasTarget = target != null;
        
        prevNode = startingNode == null? 
            GameManager.obstacleManager.GetStartingNode(nodeSet): 
            startingNode;

        currNode = prevNode.GetNextNode();
        if (currNode == null) { currNode = prevNode; }
        rb.position = prevNode.GetPos() + (Vector3.up * 2f);

        Vector3 dir = (currNode.GetPos() - rb.position).normalized;
        if (dir.sqrMagnitude > 0.001f) { transform.rotation = Quaternion.LookRotation(dir); }
    }

    private void UpdateChaseState() {
        if (!chaseTarget || target == null) return;

        float distToTarget = Vector3.Distance(rb.position, target.transform.position);
        if (!isChasing && distToTarget <= directChaseRange) { isChasing = true; }
        else if (isChasing && distToTarget > returnToNodeRange) {
            isChasing = false;
            ReattachToNodeSystem();
        }
    }

    private void AdvanceToNextNode() {
        if (currNode == null) return;
        TrafficNode next = ChooseNextNode(currNode, prevNode);
        prevNode = currNode;
        currNode = next != null ? next : currNode;
    }

    private TrafficNode ChooseNextNode(TrafficNode from, TrafficNode previous) {
        List<Pathway> pathways = from.GetPathways;
        if (pathways.Count == 0) return from;

        List<TrafficNode> candidates = new();
        foreach (Pathway pathway in pathways) {
            TrafficNode candidate = pathway.GetNextNode();
            if (candidate == previous && pathways.Count > 1) continue;
            if (candidate.IsBossNode() && !usesBossNodes) continue;
            candidates.Add(candidate);
        }

        if (candidates.Count == 0) return pathways.Count == 1 ? pathways[0].GetNextNode() : from.GetNextNode(previous);
        if (candidates.Count == 1) return candidates[0];

        if (hasTarget && target != null && graph != null) {
            TrafficNode targetNode = FindNearestNode(target.transform.position);
            if (targetNode != null) {
                TrafficNode best = null;
                float bestDist = Mathf.Infinity;
                foreach (TrafficNode candidate in candidates) {
                    float dist = graph.GetDistance(candidate, targetNode);
                    if (dist < bestDist) { bestDist = dist; best = candidate; }
                } if (best != null) return best;
            }
        }
        
        return candidates[Random.Range(0, candidates.Count)];
    }

    private TrafficNode FindBestNode(System.Func<TrafficNode, float?> scoreFn) {
    TrafficNode[] allNodes = GameManager.obstacleManager.GetNodeSet(nodeSet);
    if (allNodes == null || allNodes.Length == 0) return null;

    TrafficNode best = null;
    float bestScore = Mathf.Infinity;

    foreach (TrafficNode node in allNodes) {
        if (node == null) continue;
        float? score = scoreFn(node);
        if (score == null) continue;
        if (score.Value < bestScore) {
            bestScore = score.Value;
            best = node;
        }
    }

    return best;
}

    private TrafficNode FindNearestNode(Vector3 worldPos) => FindBestNode(node => Vector3.Distance(worldPos, node.transform.position));

    public void ReattachToNodeSystem() {
        if (currNode != null && NodeViable(currNode)) return;

        TrafficNode bestNode = FindBestNode(node => {
            if (node.IsBossNode() && !usesBossNodes) return null;
            if (node == prevNode) return null;

            float dist = Vector3.Distance(rb.position, node.transform.position);
            Vector3 dirToNode = (node.transform.position - rb.position).normalized;

            if (Physics.Raycast(rb.position + Vector3.up, dirToNode, dist, blockageMask)) return null;

            Vector3 headingRef = rb.linearVelocity.sqrMagnitude > 0.25f
                ? rb.linearVelocity.normalized
                : transform.forward;

            float dot = Vector3.Dot(headingRef, dirToNode);
            float directionalPenalty = dot >= 0f ? 1f : 2.5f;
            return dist * directionalPenalty;
        });

        if (bestNode == null) bestNode = prevNode;

        if (bestNode != null) {
            prevNode = currNode != null ? currNode : bestNode;
            currNode = bestNode;
        }
    }

    private bool NodeViable(TrafficNode node) {
        Vector3 dirToNode = node.transform.position - rb.position;
        float dist = dirToNode.magnitude;
        if (dist < 0.01f) return true;

        bool wallBlocked = Physics.Raycast(rb.position + Vector3.up, dirToNode.normalized, dist, blockageMask);
        return !wallBlocked;
    }

    public void ChangeTarget(GameObject input) {
        target = input;
        hasTarget = true;
    }
}