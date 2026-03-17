using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

// Script to handle the behaviour of the Boulder and Snowball.
public class BoulderTraversal : MonoBehaviour {
    public float speed;
    public bool snowball;
    private bool grounded, beganShrinking;
    private Rigidbody rb;
    private Vector3[] routeNodes = new Vector3[2];

    void Start() {
        rb = GetComponent<Rigidbody>();

        // Create route for the Boulder and set starting position.
        routeNodes = GameManager.instance.GetComponent<ObstacleManager>().GetClosestEdgePath();
        transform.position = new Vector3(routeNodes[0].x, 150, routeNodes[0].z);
    }

    void FixedUpdate() {
        if (!beganShrinking) { // Only move if the destination node has been reached...
            // ...and the Boulder is on the ground.
            grounded = Mathf.Round(transform.position.y) <= 15f;
            Vector3 forward = rb.rotation * Vector3.forward;
            float forwardSpeed = Vector3.Dot(forward, rb.linearVelocity);
            if (grounded && forwardSpeed < speed) { // Move Boulder towards the destiation node.
                Vector3 direction = (routeNodes[1] - transform.position).normalized;
                rb.AddForce(direction * speed, ForceMode.Acceleration);
                rb.linearVelocity *= 0.95f;
                RollRotation();
                if (snowball) { SnowballShrink(); }
            }

            // Boulder has arrived at the destination node, so begin shrinking.
            if ((rb.position - routeNodes[1]).sqrMagnitude < 25f) {
                StartCoroutine(GameManager.instance.GetComponent<ObstacleManager>().ShrinkAndDestroy(gameObject, snowball));
                // Set momentum of Boulder after it stops rolling.
                rb.linearVelocity = (routeNodes[1] - transform.position).normalized * speed; 
                rb.angularVelocity = Vector3.zero;
                beganShrinking = true;
            }
        }
    }

    // Function to give the Boulder it's "roll" rotation.
    private void RollRotation() {
        Vector3 moveDirection = (routeNodes[1] - transform.position).normalized;
        Vector3 axis = Vector3.Cross(moveDirection, Vector3.up);
        transform.Rotate(axis, -200f * Time.deltaTime, Space.World);
    }

    // Function to shrink the Snowball a small amoung every frame.
    private void SnowballShrink() {
        transform.localScale = transform.localScale - new Vector3(0.0025f, 0.0025f, 0.0025f);
        speed += 0.1f;
        rb.mass -= 0.01f;
    }
}
