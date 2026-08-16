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
    [SerializeField] private bool ignoreBlockage;

    [Header("Target Following")]
    [SerializeField] private GameObject target;
    [SerializeField] private bool followPlayer;
    [SerializeField] private bool chaseTarget;
    [SerializeField] private float directChaseRange;
    [SerializeField] private float returnToNodeRange;
    [SerializeField, Range(-1f, 1f)] private float reattachForwardBias = 0.2f;

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

        if (followPlayer) { target = GameManager.gameplayManager.GetPlayer(); }
        hasTarget = target != null;

        prevNode = GameManager.obstacleManager.GetStartingNode(nodeSet);
        if (prevNode == null) return;

        currNode = prevNode.GetNextNode();
        if (currNode == null) { currNode = prevNode; }
        rb.position = prevNode.GetPos() + (Vector3.up * 2f);

        Vector3 dir = (currNode.GetPos() - rb.position).normalized;
        if (dir.sqrMagnitude > 0.001f) { transform.rotation = Quaternion.LookRotation(dir); }
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

        UpdateTopSpeed(rayOrigin);
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

    private void UpdateTopSpeed(Vector3 rayOrigin) {
        if (!ignoreBlockage && Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, 10f, blockageMask)) {
            actTopSpeed = Mathf.Lerp(-topSpeed / 1.5f, topSpeed, hit.distance / 25f);
        } else { actTopSpeed = topSpeed; }
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
        if (CanAccelerate(forwardSpeed)) { MoveCar(); }
    }

    private bool CanAccelerate(float forwardSpeed) =>
        (actTopSpeed > 0f && forwardSpeed < actTopSpeed) ||
        (actTopSpeed < 0f && forwardSpeed > actTopSpeed);

    private void MoveCar() {
        Vector3 velocity = rb.linearVelocity;
        Vector3 right = rb.rotation * Vector3.right;
        float lateralVel = Vector3.Dot(velocity, right);
        Vector3 lateralCorrection = grip * lateralVel * -right;
        rb.AddForce(lateralCorrection, ForceMode.Acceleration);
        rb.AddForce(actTopSpeed * 3f * transform.forward, ForceMode.Acceleration);
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
        if (pathways.Count == 1) return pathways[0].GetNextNode();

        if (hasTarget && target != null) {
            TrafficNode best = null;
            float bestDist = Mathf.Infinity;
            Vector3 targetPos = target.transform.position;

            foreach (Pathway pathway in pathways) {
                TrafficNode candidate = pathway.GetNextNode();
                if (candidate == previous) continue;

                float dist = Vector3.Distance(candidate.GetPos(), targetPos);
                if (dist < bestDist) {
                    bestDist = dist;
                    best = candidate;
                }
            }
            if (best != null) return best;
        } return from.GetNextNode(previous);
    }

    private void ReattachToNodeSystem() {
        TrafficNode[] allNodes = GameManager.obstacleManager.GetNodeSet(nodeSet);
        if (allNodes == null || allNodes.Length == 0) return;

        TrafficNode previous = prevNode;
        TrafficNode bestNode = FindBestReattachNode(allNodes, requireForwardFacing: true) ?? FindBestReattachNode(allNodes, requireForwardFacing: false);
        if (bestNode == null) return;

        TrafficNode candidate = PickReattachExit(bestNode, previous);
        prevNode = bestNode;
        currNode = candidate == null ? bestNode : candidate;
    }

    private TrafficNode PickReattachExit(TrafficNode node, TrafficNode previous) {
        if (node == null) return null;

        List<Pathway> pathways = node.GetPathways;
        if (pathways == null || pathways.Count == 0) return node;

        TrafficNode bestExit = null;
        float bestScore = Mathf.Infinity;

        foreach (Pathway pathway in pathways) {
            if (pathway == null) continue;
            TrafficNode candidate = pathway.GetNextNode();
            if (candidate == null || candidate == previous || candidate == node) continue;

            Vector3 dirToExit = (candidate.GetPos() - rb.position).normalized;
            bool wallBlocked = Physics.Raycast(rb.position + Vector3.up, dirToExit, Vector3.Distance(rb.position, candidate.GetPos()) + 0.5f, blockageMask);
            if (wallBlocked) continue;

            float forwardDot = Vector3.Dot(transform.forward, dirToExit);
            float sideBias = Mathf.Abs(Vector3.Dot(transform.right, dirToExit));
            float score = (1f - Mathf.Clamp01(forwardDot)) * 8f + sideBias * 3f;

            if (score < bestScore) {
                bestScore = score;
                bestExit = candidate;
            }
        }

        if (bestExit != null) return bestExit;
        if (pathways.Count == 1) return pathways[0].GetNextNode();
        return node.GetNextNode(previous);
    }

    private TrafficNode FindBestReattachNode(TrafficNode[] allNodes, bool requireForwardFacing) {
        TrafficNode bestNode = null;
        float bestScore = Mathf.Infinity;

        foreach (TrafficNode node in allNodes) {
            if (node == null) continue;
            float dist = Vector3.Distance(rb.position, node.transform.position);
            if (dist < 0.01f) continue;

            Vector3 dirToNode = (node.GetPos() - rb.position).normalized;
            bool wallBlocked = Physics.Raycast(rb.position + Vector3.up, dirToNode, dist + 0.25f, blockageMask);
            if (wallBlocked) continue;

            float dot = Vector3.Dot(transform.forward, dirToNode);
            if (requireForwardFacing && dot < reattachForwardBias) continue;

            float sideBias = Mathf.Abs(Vector3.Dot(transform.right, dirToNode));
            float directionalPenalty = 2f - Mathf.Clamp(dot, -1f, 1f);
            float score = (dist * directionalPenalty) + (sideBias * 5f);

            if (score < bestScore) {
                bestScore = score;
                bestNode = node;
            }
        }
        return bestNode;
    }

    public void ChangeTarget(GameObject input) {
        target = input;
        hasTarget = true;
    }

    public void ChangeTopSpeed(int input) => topSpeed = input;
}