using UnityEngine;

// Script to handle control of the Mail Van.
public class PlayerCollisionController : MonoBehaviour {
    
    [SerializeField] private GameObject particleManager;

    private void OnCollisionEnter(Collision collision) {
        // Increase Crash Count for achievement tracking.
        GameManager.dataManager.IncreaseProgress(1);
        
        if (collision.relativeVelocity.magnitude > 5f) {
            // Produce Collision Particles.
            particleManager.transform.position = collision.contacts[0].point;
            particleManager.GetComponent<ParticleSystem>().Play();
        }
    }
}
