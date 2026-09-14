using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BoxingGlove : MonoBehaviour {
    [SerializeField] private float startHeight, targetHeight, speed;
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private AudioSource audioSource;
    private static readonly WaitForSeconds _waitForSeconds1 = new(1);
    private GameObject gloveObj, player;
    private Vector3 targetPosition;
    private Rigidbody rb;
    private int stage = 0;
    private int timer = 0;
    private bool playerDodged = true;

    void Awake() { 
        gloveObj = transform.GetChild(0).gameObject;
        rb = gloveObj.GetComponent<Rigidbody>(); 
    }

    void OnEnable() {
        stage = 0;
        timer = 0;
        playerDodged = true;

        player = GameManager.gameplayManager.GetPlayer();
        StartCoroutine(PunchTimer());
    }

    void OnDisable() { StopAllCoroutines(); }

    void FixedUpdate() {
        switch (stage) {
            case 0: { // Waiting
                targetPosition = player.transform.position + (player.transform.forward * 7.5f) + (Vector3.up * startHeight);
                transform.position = targetPosition;
                //rb.MovePosition(targetPosition);
                LookRotation();
                break; }
            case 1: { // Punching
                if (rb.position.y < targetHeight) {
                    ps.Play();
                    rb.MovePosition(rb.position + Vector3.up * speed);
                }
                break; }
            case 2: { // Retreating
                if (rb.position.y > startHeight) {
                    ps.Play();
                    rb.MovePosition(rb.position - Vector3.up * speed / 4);
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

    private void OnCollisionEnter(Collision collision) { if (collision.gameObject.CompareTag("Player"))  playerDodged = false; }

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
            }
            yield return _waitForSeconds1;
        }
        if (playerDodged) { GameManager.dataManager.CompleteAchievement("dodgeBoxing"); }
        gameObject.SetActive(false);
    }
}