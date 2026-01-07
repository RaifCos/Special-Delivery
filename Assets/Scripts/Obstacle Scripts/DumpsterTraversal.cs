using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

// Script to handle the behaviour of the Dumpster.
public class DumpsterTraversal : MonoBehaviour
{
    public float speed;

    private bool grounded, beganShrinking;
    private Rigidbody rb;
    private Vector3[] routeNodes = new Vector3[2];

    void Start() {
        rb = GetComponent<Rigidbody>();

        // Create route for the Dumpster and set starting position.
        routeNodes = GameManager.instance.GetComponent<ObstacleManager>().GetEdgePath();
        transform.position = new Vector3(routeNodes[0].x, 150, routeNodes[0].z);
    }

    void FixedUpdate() {
        if (!beganShrinking) { // Only move if the destination node has been reached...
            // ...and the Dumpster is on the ground.
            grounded = transform.position.y <= 8f;
            if (grounded) { 
                rb.MovePosition(Vector3.MoveTowards(transform.position, routeNodes[1], speed * Time.deltaTime));
                speed += speed * 0.005f; // Increase Speed every frame.
                LookRotation();
            }

            // Dumpster has arrived at the destination node, so begin shrinking.
            if (Vector3.Distance(rb.position, routeNodes[1]) < 3f) {
                StartCoroutine(GameManager.instance.GetComponent<ObstacleManager>().ShrinkAndDestroy(this.gameObject, false));
                // Set momentum of Dumpster after it stops moving.
                rb.linearVelocity = (routeNodes[1] - transform.position).normalized * speed; 
                rb.angularVelocity = Vector3.zero;
                beganShrinking = true;
            }
        }
    }

    // Function to set the rotation of the Dumpster to face the destination node.
    private void LookRotation() {
        Vector3 direction = (routeNodes[1] - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 3f * Time.deltaTime);
    }
}
