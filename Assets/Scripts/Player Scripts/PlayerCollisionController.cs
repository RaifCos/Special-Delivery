using UnityEngine;

// Script to handle control of the Mail Van.
public class PlayerCollisionController : MonoBehaviour {

    [SerializeField] private AudioSource[] crashSoundSources;
    [SerializeField] private AudioClip[] defaultSounds;
    [SerializeField] private GameObject particleManager;
    private AudioClip[] soundArray;
    
    private void OnCollisionEnter(Collision collision) {
        // Increase Crash Count for achievement tracking.
        GameManager.dataManager.IncreaseProgress(1);
        
        if (collision.relativeVelocity.magnitude > 5f) {
            // Produce Collision Particles.
            particleManager.transform.position = collision.contacts[0].point;
            particleManager.GetComponent<ParticleSystem>().Play();

            // Retrieve an Audio Source currently not in use. 
            AudioSource css = null;
            foreach (var soundSource in crashSoundSources) {
                if (!soundSource.isPlaying) {
                    css = soundSource;
                    break;
                }
            }

            if (css != null) {
                // Retrieve sound array from colliding object. 
                CollisionSounds cs = collision.gameObject.GetComponent<CollisionSounds>();
                if (cs == null) { soundArray = defaultSounds; }
                else { soundArray = cs.GetSoundArray(); }

                // Select random clip from sound array, randomize pitch and play. 
                css.clip = soundArray[Random.Range(0, soundArray.Length)];
                css.pitch = Random.Range(0.8f, 1.1f);
                css.Play();
            }
        }
    }
}
