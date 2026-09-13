using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarMovement))]
public class CarTraversal : MonoBehaviour {
    [Header("Car Variables")]
    [SerializeField] private float topSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float grip;
    [SerializeField] private float distanceThreshold;
    [SerializeField] private float height;
    private float actTopSpeed;

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

    [Header("Collision Reaction")]
    [SerializeField] private bool ignoreStun; 
    [SerializeField] private float minImpactForce = 5f;    
    [SerializeField] private float stunDuration = 0.6f;       
    [SerializeField] private float bounceForceMultiplier = 0.02f;
    [SerializeField] private float maxBounceForce = 12f;
    [SerializeField] private float spinTorque = 6f;
    [SerializeField] private float collisionCooldown = 0.15f;

    private float stunTimer;
    private float collisionCooldownTimer;

    private bool hasTarget; 
    private bool isChasing;

    private Rigidbody rb;
    private CarMovement cM;
    private TrafficNode currNode, prevNode;
    private NodeGraph graph;
    private LayerMask blockageMask, roadMask;

    void Start() {
        rb = GetComponent<Rigidbody>();
        cM = GetComponent<CarMovement>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        blockageMask = LayerMask.GetMask("Blockage");
        roadMask = LayerMask.GetMask("Road");
        graph = GameManager.obstacleManager.GetGraph(nodeSet);
        Initialize();
    }

    void FixedUpdate() {
        if (collisionCooldownTimer > 0f) { collisionCooldownTimer -= Time.fixedDeltaTime; }

        if (cM.IsStunned) {
            stunTimer -= Time.fixedDeltaTime;
            if (stunTimer <= 0f) { ReattachToNodeSystem(); }
            return; 
        }

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

        if (candidates.Count == 0) {
            return pathways.Count == 1 ? pathways[0].GetNextNode() : from.GetNextNode(previous);
        } if (candidates.Count == 1) return candidates[0];

        if (hasTarget && target != null && graph != null) {
            TrafficNode targetNode = FindNearestNode(target.transform.position);
            if (targetNode != null) {
                TrafficNode best = null;
                float bestDist = Mathf.Infinity;
                foreach (TrafficNode candidate in candidates) {
                    float dist = graph.GetDistance(candidate, targetNode);
                    if (dist < bestDist) { bestDist = dist; best = candidate; }
                }
                if (best != null) return best;
            }
        }
        return candidates[Random.Range(0, candidates.Count)];
    }

    private TrafficNode FindNearestNode(Vector3 worldPos) {
        TrafficNode[] allNodes = GameManager.obstacleManager.GetNodeSet(nodeSet);
        if (allNodes == null || allNodes.Length == 0) return null;

        TrafficNode nearest = null;
        float bestDist = Mathf.Infinity;
        foreach (TrafficNode node in allNodes) {
            if (node == null) continue;
            float dist = Vector3.Distance(worldPos, node.transform.position);
            if (dist < bestDist) { bestDist = dist; nearest = node; }
        }
        return nearest;
    }
    
    public void ReattachToNodeSystem() {
        // Skip Reattachment if the vehicle can still reach it's current target.
        if (currNode != null && NodeViable(currNode)) return;

        TrafficNode[] allNodes = GameManager.obstacleManager.GetNodeSet(nodeSet);
        if (allNodes == null || allNodes.Length == 0) return;

        TrafficNode bestNode = null;
        float bestScore = Mathf.Infinity;

        foreach (TrafficNode node in allNodes) {
            if (node == null) continue;
            if (node.IsBossNode() && !usesBossNodes) continue;
            if (node == prevNode) continue;

            float dist = Vector3.Distance(rb.position, node.transform.position);
            Vector3 dirToNode = (node.transform.position - rb.position).normalized;

            bool wallBlocked = Physics.Raycast(rb.position + Vector3.up, dirToNode, dist, blockageMask);
            if (wallBlocked) continue;

            Vector3 headingRef = rb.linearVelocity.sqrMagnitude > 0.25f
                ? rb.linearVelocity.normalized
                : transform.forward;

            float dot = Vector3.Dot(headingRef, dirToNode);
            float directionalPenalty = dot >= 0f ? 1f : 2.5f;
            float score = dist * directionalPenalty;

            if (score < bestScore) {
                bestScore = score;
                bestNode = node;
            }
        }

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

    public void SetStunTimer(float input) => stunTimer = input;  
}