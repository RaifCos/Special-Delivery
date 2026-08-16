using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GiantHammer : MonoBehaviour {

    [SerializeField] private float startHeight;
    [SerializeField] private float riseSpeed;   
    [SerializeField] private float swingSpeed;
    private int stage;
    private Rigidbody rb;
    private Quaternion baseRotation;
    private float swingAngle;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void OnEnable() {
        stage = -1;
        TrafficNode startNode = GameManager.obstacleManager.GetNearestNode(1, 20f);
        TrafficNode facingNode = startNode.GetNextNode(startNode);

        Vector3 startPos = startNode.GetPos() + (Vector3.up * startHeight);
        rb.position = startPos;
        transform.position = startPos;

        Vector3 direction = facingNode.transform.position - startNode.GetPos();
        direction.y = 0f;
        baseRotation = Quaternion.LookRotation(direction);
        rb.rotation = baseRotation;
        transform.rotation = baseRotation;

        swingAngle = 0f;
        stage = 0;
    }

    void FixedUpdate() {
        float dt = Time.fixedDeltaTime;

        switch (stage) {
            case 0: { // Descending
                if (rb.position.y < 2.5f) {
                    rb.MovePosition(rb.position + dt * riseSpeed * Vector3.up);
                } else { stage++; }
                break; }
            case 1: { // Swinging Down
                if (swingAngle < 80f) {
                    rb.isKinematic = false;
                    swingAngle += swingSpeed * dt;
                    rb.MoveRotation(baseRotation * Quaternion.Euler(swingAngle, 0f, 0f));
                } else { stage++; }
                break; }
            case 2: { // Swinging Up
                if (swingAngle > 0f) {
                    rb.isKinematic = true;
                    swingAngle -= (swingSpeed / 2f) * dt;
                    rb.MoveRotation(baseRotation * Quaternion.Euler(swingAngle, 0f, 0f));
                } else { stage++; }
                break; }
            case 3: { // Ascending
                if (rb.position.y > startHeight) {
                    rb.MovePosition(rb.position - dt * (riseSpeed / 2f) * Vector3.up);
                } else { gameObject.SetActive(false); }
                break; }
        }
    }
}