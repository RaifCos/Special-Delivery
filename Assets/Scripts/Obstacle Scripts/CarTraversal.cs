using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

// Script to handle the behaviour of the Red Car, Green Car, Blue Car, and Toy Car.
public class CarTraversal : MonoBehaviour {
    public float speed, rotationSpeed, height;
    public int nodeSet;
    private Rigidbody rb;
    GameObject currNode, prevNode;
    private Vector3 rayPoint;
    private Vector2 floorNodePos, floorPos;
    LayerMask layerMask;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
        layerMask = LayerMask.GetMask("Blockage");
        // Set inital route Nodes.
        prevNode = GameManager.obstacleManager.GetStartingNode(nodeSet);
        currNode = prevNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
        // Set movement factors based on nodeSet (0 for regular Cars, 1 for Big Car).
        rb.position = prevNode.transform.position;
        floorNodePos = new(currNode.transform.position.x, currNode.transform.position.z);
    }

    // Update is called once per frame
    void FixedUpdate() {
        if(rb.position.y < height) {
            float actSpeed = speed;
            rayPoint = transform.position + Vector3.up * 2;
            // Check if Car is close enough to its target node.
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(rayPoint, transform.forward, out RaycastHit hit, 25f, layerMask)) {
                actSpeed = Mathf.Lerp(-speed / 2f, speed, hit.distance / 25f);
            }

            floorPos = new(rb.position.x, rb.position.z);
            if (Vector2.Distance(floorPos, floorNodePos) > 3f) {
                LookRotation();
                Vector3 direction = rb.rotation * Vector3.forward;
                Vector3 velocity = actSpeed * direction;
                velocity.y = rb.linearVelocity.y;
                rb.linearVelocity = velocity;
            } else {
                // Car is close enough to target, so select new target node.
                GameObject tempNode = currNode;
                currNode = tempNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
                prevNode = tempNode;
                floorNodePos = new(currNode.transform.position.x, currNode.transform.position.z);
            }
        }
    }

    // Function to set the rotation of the Car to face the destination node.
    private void LookRotation() {
        Vector2 direction2D = (floorNodePos - floorPos).normalized;
        Vector3 direction = new(direction2D.x, 0f, direction2D.y);
        
        if (direction.sqrMagnitude > 0.001f) {
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        rb.MoveRotation(smoothedRotation);
        }
    }
}
