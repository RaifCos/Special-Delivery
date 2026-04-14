using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

// Script to handle the behaviour of the Hot Air Balloon.
public class HotAirBalloon : MonoBehaviour {

    [SerializeField] private float moveSpeed, hoverSpeed;
    [SerializeField] private float highestPoint, lowestPoint;
    private bool goingUp;
    private Rigidbody rb;
    private Vector3 normalNodePos;
    GameObject currNode, prevNode;
    Vector3 currPos;

    void OnEnable() {
        rb = GetComponent<Rigidbody>();
        prevNode = GameManager.instance.GetComponent<ObstacleManager>().GetStartingNode(1);
        currNode = prevNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
        currPos = currNode.transform.position;
        transform.position = prevNode.transform.position + (Vector3.up * 19f);
    }

    void FixedUpdate() {
        Vector3 flatTarget = new(currPos.x, transform.position.y, currPos.z);
        normalNodePos = flatTarget;

        if (Vector3.Distance(transform.position, normalNodePos) > 7f) {
            Vector3 flatDirection = (flatTarget - transform.position).normalized;
            rb.AddForce(flatDirection * moveSpeed, ForceMode.Acceleration);
        }
        else {
            GameObject tempNode = currNode;
            currNode = tempNode.GetComponent<TrafficNode>().GetNextNode(prevNode);
            prevNode = tempNode;
            currPos = currNode.transform.position;
        }

        LookRotation();
        Hover();
        rb.linearVelocity *= 0.95f;
    }

    private void LookRotation() {
        Vector3 direction = (normalNodePos - rb.position).normalized;
        if (direction.sqrMagnitude > 0.001f) {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, 5f * Time.fixedDeltaTime);
            rb.MoveRotation(smoothedRotation);
        }
    }

    private void Hover() {
        if (transform.position.y >= highestPoint) { goingUp = false; }
        else if (transform.position.y <= lowestPoint) { goingUp = true; }
        Vector3 force = goingUp ? Vector3.up : Vector3.down;
        rb.AddForce(force * hoverSpeed, ForceMode.Acceleration);
    }
}
