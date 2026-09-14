using UnityEngine;

public class BoxingGloveHitCheck : MonoBehaviour {
    private void OnCollisionEnter(Collision collision) { if (collision.gameObject.CompareTag("Player")) transform.parent.GetComponent<BoxingGlove>().PlayerHasBeenHit(); }
}
