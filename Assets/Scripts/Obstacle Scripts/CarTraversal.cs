using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
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
    private bool IsStunned => stunTimer > 0f;

    private bool hasTarget; 
    private bool isChasing;

    private Rigidbody rb;
    private TrafficNode currNode, prevNode;
    private LayerMask blockageMask, roadMask;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        blockageMask = LayerMask.GetMask("Blockage");
        roadMask = LayerMask.GetMask("Road");
        Initialize();
    }

    void FixedUpdate() {
        if (collisionCooldownTimer > 0f) { collisionCooldownTimer -= Time.fixedDeltaTime; }

        if (IsStunned) {
            stunTimer -= Time.fixedDeltaTime;
            if (stunTimer <= 0f) { ReattachToNodeSystem(); }
            return; 
        }

        Vector3 rayOrigin = transform.position + Vector3.up;
        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit roadHit, height, roadMask)) return;

        if (!ignoreBlockage && Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, 10f, blockageMask)) {
            actTopSpeed = Mathf.Lerp(-topSpeed / 1.5f, topSpeed, hit.distance / 25f);
        } else { actTopSpeed = topSpeed; }
        
        UpdateChaseState();

        if (isChasing) {
            if (target == null) return;
            DriveToward(roadHit.normal, target.transform.position);
            return;
        }

        if (currNode == null) return;
        if (Vector3.Distance(rb.position, currNode.transform.position) > distanceThreshold) {
            DriveToward(roadHit.normal, currNode.transform.position);
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

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Level")
        || collisionCooldownTimer > 0f
        || collision.relativeVelocity.magnitude < minImpactForce
        || ignoreStun
        ) return; 

        float impactForce = collision.impulse.magnitude / Time.fixedDeltaTime;
        if (impactForce < minImpactForce) return;

        collisionCooldownTimer = collisionCooldown;
        stunTimer = stunDuration;

        ContactPoint contact = collision.GetContact(0);

        Vector3 pushDir = contact.normal;
        pushDir.y = 0f;
        if (pushDir.sqrMagnitude > 0.0001f) {
            float pushForce = Mathf.Min(impactForce * bounceForceMultiplier, maxBounceForce);
            rb.AddForce(pushDir.normalized * pushForce, ForceMode.Impulse);
        }

        float side = Vector3.Dot(transform.right, contact.point - transform.position) >= 0f ? -1f : 1f;
        float torqueScale = Mathf.Clamp01(impactForce / (minImpactForce * 4f));
        rb.AddTorque(side * spinTorque * torqueScale * Vector3.up, ForceMode.Impulse);
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

    private void DriveToward(Vector3 surfaceNormal, Vector3 targetPosition) {
        LookRotation(surfaceNormal, targetPosition);
        float forwardSpeed = Vector3.Dot(rb.rotation * Vector3.forward, rb.linearVelocity);
        if (actTopSpeed > 0f && forwardSpeed < actTopSpeed || actTopSpeed < 0f && forwardSpeed > actTopSpeed) {
            Vector3 right = rb.rotation * Vector3.right;
            float lateralVel = Vector3.Dot(rb.linearVelocity, right);
            Vector3 lateralCorrection = grip * lateralVel * -right;
            rb.AddForce(lateralCorrection, ForceMode.Acceleration);
            rb.AddForce(actTopSpeed * 3f * transform.forward, ForceMode.Acceleration);
        } 
    }

    private void LookRotation(Vector3 surfaceNormal, Vector3 targetPosition) {
        Vector3 direction = (targetPosition - rb.position).normalized;
        if (direction.sqrMagnitude <= 0.001f) return;

        Vector3 surfaceForward = Vector3.ProjectOnPlane(direction, surfaceNormal).normalized;
        if (surfaceForward.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(surfaceForward, surfaceNormal);
        Quaternion smoothedRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothedRotation);
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

        if (hasTarget && target != null) {
            TrafficNode best = null;
            float bestDist = Mathf.Infinity;
            Vector3 targetPos = target.transform.position;
            foreach (TrafficNode candidate in candidates) {
                float dist = Vector3.Distance(candidate.GetPos(), targetPos);
                if (dist < bestDist) { bestDist = dist; best = candidate; }
            } return best;
        } return candidates[Random.Range(0, candidates.Count)];
    }
    
    private void ReattachToNodeSystem() {
        TrafficNode[] allNodes = GameManager.obstacleManager.GetNodeSet(nodeSet);
        if (allNodes == null || allNodes.Length == 0) return;

        TrafficNode bestNode = null;
        float bestScore = Mathf.Infinity;

        foreach (TrafficNode node in allNodes) {
            if (node == null) continue;
            if (node.IsBossNode() && !usesBossNodes) continue;
            float dist = Vector3.Distance(rb.position, node.transform.position);

            Vector3 dirToNode = (node.transform.position - rb.position).normalized;
            bool wallBlocked = Physics.Raycast(rb.position + Vector3.up, dirToNode, dist, blockageMask);
            if (wallBlocked) continue;

            float dot = Vector3.Dot(transform.forward, dirToNode);
            float directionalPenalty = dot >= 0f ? 1f : 2.5f;
            float score = dist * directionalPenalty;

            if (score < bestScore) {
                bestScore = score;
                bestNode = node;
            }
        }

        if (bestNode != null) {
            prevNode = bestNode;
            currNode = bestNode;
            if (currNode == null) currNode = bestNode;
        }
    }

    public void ChangeTarget(GameObject input) {
        target = input;
        hasTarget = true;
    }

    public void ChangeTopSpeed(int input) => topSpeed = input;
}