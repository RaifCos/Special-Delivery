using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

// Script to handle the behaviour of the Red Car, Green Car, Blue Car, and Toy Car.
public class CarTraversal : MonoBehaviour {
    [SerializeField] private float topSpeed;
    [SerializeField] private float grip;
    [SerializeField] private float distanceThreshold;
    [SerializeField] private float height;
    [SerializeField] private int nodeSet;
    [SerializeField] private bool ignoreBlockage;
    private float actTopSpeed;
    private Rigidbody rb;
    private GameObject currNode, prevNode;
    private Vector3 rayPoint, forward;
    LayerMask layerMask;

    // Start is called before the first frame update
    void OnEnable() {
        rb = GetComponent<Rigidbody>();
        layerMask = LayerMask.GetMask("Blockage");
        // Set inital route Nodes.
        prevNode = GameManager.obstacleManager.GetStartingNode(nodeSet);
        currNode = prevNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
        // Set movement factors based on nodeSet (0 for regular Cars, 1 for Big Car).
        rb.position = prevNode.transform.position + (Vector3.up * 2f);
    }

    // Update is called once per frame
    void FixedUpdate() {
        if(rb.position.y < height) {
            actTopSpeed = topSpeed;
            rayPoint = transform.position + Vector3.up * 2;

            if (Physics.Raycast(rayPoint, transform.forward, out RaycastHit hit, 25f, layerMask) && !ignoreBlockage) {
                actTopSpeed = Mathf.Lerp(-topSpeed / 2f, topSpeed, hit.distance / 25f);
            } else { actTopSpeed = topSpeed; }

            Vector3 forward = rb.rotation * Vector3.forward;
            float forwardSpeed = Vector3.Dot(forward, rb.linearVelocity);
            if (Vector3.Distance(rb.position, currNode.transform.position) > distanceThreshold) {
                LookRotation();
                if(forwardSpeed < actTopSpeed) { MoveCar(); }
            } else { UpdateNode(); }
        }
    }

    private void MoveCar() {
        Vector3 velocity = rb.linearVelocity;
        Vector3 right   = rb.rotation * Vector3.right;
            
        // Decompose velocity
        float forwardVel = Vector3.Dot(velocity, forward);
        float lateralVel = Vector3.Dot(velocity, right);

        // Kill lateral sliding
        Vector3 lateralCorrection = grip * lateralVel * -right;
        rb.AddForce(lateralCorrection, ForceMode.Acceleration);
        rb.AddForce(actTopSpeed * 3f * transform.forward, ForceMode.Acceleration);
    }

    // Function to set the rotation of the Car to face the destination node.
    private void LookRotation() {
        Vector3 direction = (currNode.transform.position - rb.position).normalized;
        if (direction.sqrMagnitude > 0.001f) {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, 5f * Time.fixedDeltaTime);
            rb.MoveRotation(smoothedRotation);
        }
    }

    private void UpdateNode() {
        GameObject tempNode = currNode;
        currNode = tempNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
        prevNode = tempNode;
    }
}