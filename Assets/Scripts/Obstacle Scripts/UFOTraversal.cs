using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

// Script to handle the behaviour of the UFO.
public class UFOTraversal : MonoBehaviour {

    [SerializeField] private float speed;
    [SerializeField] private float height;
    private Rigidbody rb;
    GameObject currNode, prevNode;
    Vector3 currPos;

    void Start() {
        rb = GetComponent<Rigidbody>();

        prevNode = GameManager.instance.GetComponent<ObstacleManager>().GetStartingNode(1);
        currNode = prevNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
        currPos = currNode.transform.position + (Vector3.up * height);
        transform.position = prevNode.transform.position + (Vector3.up * height);
    }

    void FixedUpdate() {
        if ((transform.position - currPos).sqrMagnitude > 9f) {
            Vector3 direction = (currPos - transform.position).normalized;
            rb.AddForce(direction * speed, ForceMode.Acceleration);
            rb.linearVelocity *= 0.95f;
            SpinRotation();
        }
        else {
            GameObject tempNode = currNode;
            currNode = tempNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
            prevNode = tempNode;
            currPos = currNode.transform.position + (Vector3.up * height);
        }
    }

    // Function to constantly rotate the UFO.
    public void SpinRotation() { 
        rb.MoveRotation( rb.rotation * Quaternion.Euler(20f * Time.fixedDeltaTime * Vector3.up) );
    }
}
