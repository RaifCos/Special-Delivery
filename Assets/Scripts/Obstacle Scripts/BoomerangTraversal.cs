using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

// Script to handle the behaviour of the Boomerang.
public class BoomerangTraversal : MonoBehaviour
{
    // Variable to set the base speed.
    public float speed;

    private float actSpeed;
    private Rigidbody rb;
    private Vector3[] routeNodes = new Vector3[2];
    private Vector3 midpoint;
    private float midDist;

    void OnEnable() {
        rb = GetComponent<Rigidbody>();

        // Create route for the Boomerang and set starting position. 
        routeNodes = GameManager.instance.GetComponent<ObstacleManager>().GetEdgePath();
        transform.position = routeNodes[0] + Vector3.up;

        // Find the mid point of the Boomerang's route, and the distance to reach it. 
        midpoint = Vector3.Lerp(routeNodes[0], routeNodes[1], 0.5f);
        midDist = Vector3.Distance(routeNodes[0], midpoint);
    }

    void FixedUpdate() {
        // Update speed so the boomerang is faster as it approaches the midpoint.
        float distToMid = Vector3.Distance(rb.position, midpoint);
        float t = 1f - Mathf.Clamp01(distToMid / midDist);
        actSpeed = Mathf.Max(0f, speed + t * 10f) * Time.fixedDeltaTime;

        // Move and Rotate Boomerang.
        rb.MovePosition(Vector3.MoveTowards(transform.position, routeNodes[1], actSpeed));
        Quaternion newRotation = Quaternion.Euler(0f, 350f * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * newRotation);

        // Reset route when Boomerang approaches destination.
        if (Vector3.Distance(rb.position, routeNodes[1]) < 3f) {
            (routeNodes[1], routeNodes[0]) = (routeNodes[0], routeNodes[1]);
        }
    }
}
