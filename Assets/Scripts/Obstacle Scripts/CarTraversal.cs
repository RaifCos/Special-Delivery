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
    [SerializeField] private int depth = 1;

    [SerializeField] private float goalRecalcThreshold = 5f;
    private Vector3 lastGoalCalcPosition;
    private bool goalPositionDirty = true;

    private bool followingTarget, chasingTarget;
    private Rigidbody rb;
    private TrafficNode currNode, prevNode;
    private LayerMask layerMask, roadMask;

    void OnEnable() {
        rb = GetComponent<Rigidbody>();
        layerMask = LayerMask.GetMask("Blockage");
        roadMask = LayerMask.GetMask("Road");
    }

    void Start() {
        if (followPlayer) { target = GameManager.gameplayManager.GetPlayer(); }
        followingTarget = target != null;
        chasingTarget = false;

        prevNode = GameManager.obstacleManager.GetStartingNode(nodeSet);
        if (prevNode != null) {
            currNode = prevNode.GetNextNode();
            if (currNode == null) { currNode = prevNode; }
            rb.position = prevNode.GetPos() + (Vector3.up * 2f);

            Vector3 dir = (currNode.GetPos() - rb.position).normalized;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void FixedUpdate() {
        Vector3 rayPoint = transform.position + Vector3.up;
        if (Physics.Raycast(rayPoint, Vector3.down, out RaycastHit roadHit, height, roadMask)) {

            if (!ignoreBlockage && Physics.Raycast(rayPoint, transform.forward, out RaycastHit hit, 10f, layerMask)) {
                actTopSpeed = Mathf.Lerp(-topSpeed / 2f, topSpeed, hit.distance / 25f);
            } else { actTopSpeed = topSpeed; }

            Vector3 forward = rb.rotation * Vector3.forward;
            float forwardSpeed = Vector3.Dot(forward, rb.linearVelocity);

            ChaseUpdate();

            if (chasingTarget) {
                if (target == null) { return; }
                LookRotation(roadHit.normal, target.transform.position);
                if ((actTopSpeed > 0f && forwardSpeed < actTopSpeed) || (actTopSpeed < 0f && forwardSpeed > actTopSpeed))
                    MoveCar();
            } else {
                if (currNode == null) { return; }
                if (Vector3.Distance(rb.position, currNode.transform.position) > distanceThreshold) {
                    LookRotation(roadHit.normal, currNode.transform.position);
                    if ((actTopSpeed > 0f && forwardSpeed < actTopSpeed) || (actTopSpeed < 0f && forwardSpeed > actTopSpeed))
                        MoveCar();
                } else { UpdateNode(); }
            }
        }
    }

    private void ChaseUpdate() {
        if (!chaseTarget || target == null) return;

        float distToTarget = Vector3.Distance(rb.position, target.transform.position);
        if (!chasingTarget && distToTarget <= directChaseRange) {
            chasingTarget = true;
        } else if (chasingTarget && distToTarget > returnToNodeRange) {
            chasingTarget = false;
            ReattachToNodeSystem();
        }
    }

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
        if (direction.sqrMagnitude > 0.001f) {
            Vector3 surfaceForward = Vector3.ProjectOnPlane(direction, surfaceNormal).normalized;
            if (surfaceForward.sqrMagnitude > 0.001f) {
                Quaternion targetRotation = Quaternion.LookRotation(surfaceForward, surfaceNormal);
                Quaternion smoothedRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
                rb.MoveRotation(smoothedRotation);
            }
        }
    }

    private void UpdateNode() {
        TrafficNode tempNode = currNode;
        if (tempNode == null) { return; }

        if (followingTarget && target != null) {
            if (goalPositionDirty ||
                Vector3.Distance(target.transform.position, lastGoalCalcPosition) > goalRecalcThreshold) {
                lastGoalCalcPosition = target.transform.position;
                goalPositionDirty = false;
            }
        }

        if (followingTarget) {
            Vector3 targetPosition = target != null ? target.transform.position : tempNode.transform.position;
            // currNode == tempNode.GetNextClosestNode();
        } else {
            currNode = tempNode.GetNextNode();
        }

        if (currNode == null) { currNode = tempNode; }
        prevNode = tempNode;
    }

    private void ReattachToNodeSystem() {
        TrafficNode[] allNodes = GameManager.obstacleManager.GetNodeSet(nodeSet);
        if (allNodes == null || allNodes.Length == 0) return;

        TrafficNode bestNode = null;
        float bestScore = Mathf.Infinity;

        foreach (TrafficNode node in allNodes) {
            if (node == null) continue;
            float dist = Vector3.Distance(rb.position, node.transform.position);

            Vector3 dirToNode = (node.GetPos() - rb.position).normalized;
            bool wallBlocked = Physics.Raycast(rb.position + Vector3.up, dirToNode, dist, layerMask);
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
            currNode = bestNode.GetNextNode();
            if (currNode == null) currNode = bestNode;

            goalPositionDirty = true;
        }
    }

    public void ChangeTarget(GameObject input) {
        target = input;
        followingTarget = true;
        goalPositionDirty = true;
    }

    public void ChangeTopSpeed(int input) => topSpeed = input;
}