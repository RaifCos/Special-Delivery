using UnityEngine;

// Script to handle control of the Mail Van.
public class PlayerCollisionController : MonoBehaviour {

    public AudioSource[] crashSoundSources;
    public AudioClip[] soundEffects;
    public GameObject particleManager;

    Rigidbody rb;
    
    private void Start() {
        rb = gameObject.GetComponent<Rigidbody>(); // Get Rigidbody component.
    }
    
    // Function to play a collision sound when colliding with another object.
    private void OnCollisionEnter(Collision collision) {
        // Increase crash count (for achievement tracking).
        GameManager.achievementManager.IncreaseProgress(1);

        if (!collision.gameObject.CompareTag("Mute")) {
            // To handle multiple collisions in short sucession, use two Crash Sound Sources.
            AudioSource crashSound = null;
            foreach (var soundSource in crashSoundSources) {
                if (!soundSource.isPlaying) {
                    crashSound = soundSource;
                    break;
                }
            }

            // Only play sound if a Sound Source is available and the van is going fast enough.
            if (crashSound != null && collision.relativeVelocity.magnitude > 5f) {

                particleManager.transform.position = collision.contacts[0].point;
                particleManager.GetComponent<ParticleSystem>().Play();

                // Play appropiate sound based on colliding object. 0
                if (collision.gameObject.CompareTag("Car") || collision.gameObject.CompareTag("Level")) { crashSound.clip = soundEffects[1]; }
                else if (collision.gameObject.CompareTag("Cone")) { crashSound.clip = soundEffects[2]; }
                else { crashSound.clip = soundEffects[0]; } // No sound set for this object, play a default sound.

                // Randomize pitch of sound for variety and play.
                crashSound.pitch = Random.Range(0.8f, 1.1f);
                crashSound.Play();
            }
        }
    }
}
