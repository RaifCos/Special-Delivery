using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(Rigidbody))]

// Script to handle the behaviour of the Red Car, Green Car, Blue Car, and Toy Car.
public class CarTraversal : MonoBehaviour {
    [SerializeField] private float topSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float grip;
    [SerializeField] private float distanceThreshold;
    [SerializeField] private float height;
    [SerializeField] private int nodeSet;
    [SerializeField] private bool ignoreBlockage;
    [SerializeField] private GameObject target;
    [SerializeField] private bool followPlayer;
    private float actTopSpeed;
    private bool followingTarget;
    private Rigidbody rb;
    private GameObject currNode, prevNode;
    LayerMask layerMask, roadMask;

    void OnEnable() {
        rb = GetComponent<Rigidbody>();
        layerMask = LayerMask.GetMask("Blockage");
        roadMask = LayerMask.GetMask("Road");
    }
    void Start() {
        // Set if Car should follow a Target Object.
        if (followPlayer) { target = GameManager.gameplayManager.GetPlayer(); }
        followingTarget = target != null;

        // Set inital route Nodes.
        prevNode = GameManager.obstacleManager.GetStartingNode(nodeSet);
        currNode = prevNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
        rb.position = prevNode.transform.position + (Vector3.up * 2f);
        transform.rotation = Quaternion.Euler((currNode.transform.position - rb.position).normalized);
    }

    void FixedUpdate() {
        // Check Car is Grounded.
        Vector3 rayPoint = transform.position + Vector3.up;
        if (Physics.Raycast(rayPoint, Vector3.down, out RaycastHit roadHit, height, roadMask)) {

            // Stop/Reverse if there is traffic in front of the Car.
            if (!ignoreBlockage && Physics.Raycast(rayPoint, transform.forward, out RaycastHit hit, 10f, layerMask)) {
                actTopSpeed = Mathf.Lerp(-topSpeed / 2f, topSpeed, hit.distance / 25f);
            } else { actTopSpeed = topSpeed; }

            // Set Forward Direction and Speed.
            Vector3 forward = rb.rotation * Vector3.forward;
            float forwardSpeed = Vector3.Dot(forward, rb.linearVelocity);

            // If the Car is at the target node, move onto the next node. Otherwise, turn and drive towards it.
            if (Vector3.Distance(rb.position, currNode.transform.position) > distanceThreshold) {
                LookRotation(roadHit.normal);
                if ((actTopSpeed > 0f && forwardSpeed < actTopSpeed) || (actTopSpeed < 0f && forwardSpeed > actTopSpeed))
                { MoveCar(); }
            } else { UpdateNode(); }
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

    private void LookRotation(Vector3 surfaceNormal) {
        Vector3 direction = (currNode.transform.position - rb.position).normalized;
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
        GameObject tempNode = currNode;
        if (followingTarget) { 
            Vector3 targetPosition = target.transform.position;
            currNode = tempNode.GetComponent<TrafficNode>().GetNextClosestNode(prevNode, targetPosition);
        } else { currNode = tempNode.GetComponent<TrafficNode>().GetNextNode(prevNode); }
        prevNode = tempNode;
    }

    public void ChangeTarget(GameObject input) { 
        target = input;
        followingTarget = true;
    }

    public void ChangeTopSpeed(int input) => topSpeed = input;
}