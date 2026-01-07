using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoxingGlove : MonoBehaviour {
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1);
    public float startHeight, targetHeight, speed;
    public AudioSource audioSource;
    private GameObject player;
    private int stage = 0;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private int timer = 0;


    void Start() {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(PunchTimer());
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void FixedUpdate() {
        switch (stage) {
            case 0: { // Waiting
                targetPosition = player.transform.position + (player.transform.forward * 7.5f) + (Vector3.up * startHeight);
                rb.MovePosition(targetPosition);
                LookRotation();
                break; }
            case 1: { // Punch
                    if (rb.position.y < targetHeight) {
                        rb.MovePosition(rb.position + Vector3.up * speed);
                    }
                break; }
            case 2: { // Retreat
                    if (rb.position.y > startHeight) {
                        rb.MovePosition(rb.position - Vector3.up * speed/4);
                    } else { stage++; }
                break; }
        }
    }

    private void LookRotation() {
        Vector2 direction2D = (player.transform.position - rb.position).normalized;
        Vector3 direction = new(direction2D.x, 0f, direction2D.y);
        
        if (direction.sqrMagnitude > 0.001f) {
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        rb.MoveRotation(targetRotation);
        }
    }

    IEnumerator PunchTimer() {
        while (stage < 3) {
            timer++;
            switch (timer) {
                case 2: {
                    audioSource.Play();
                    break; }
                case 3: 
                case 5: {
                    stage++;
                    break; }
            } yield return _waitForSeconds1;
        } Destroy(gameObject);
    }
}
