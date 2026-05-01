using UnityEngine;

// Script to handle the sounds emitted by Obstacles when they collide with another object.
public class CollisionSounds : MonoBehaviour {
    [SerializeField] private AudioClip[] collisionSoundEffects;
    [SerializeField] private AudioClip[] triggerSoundEffects;
    [SerializeField] private bool includeGround; 
    [SerializeField] private float colliderPitchOffset; 
    [SerializeField] private float triggerPitchOffset; 
    [SerializeField] private bool randomisePitch;
    private readonly bool[] hasSoundEffects = new bool[2];

    private void Start() {
        hasSoundEffects[0] = collisionSoundEffects.Length > 0;
        hasSoundEffects[1] = triggerSoundEffects.Length > 0;
    }

    private void OnCollisionEnter(Collision collision) {
        if (hasSoundEffects[0] && (includeGround || !collision.gameObject.CompareTag("Level"))) {
            AudioClip clip = collisionSoundEffects[Random.Range(0, collisionSoundEffects.Length)];
            GameManager.audioManager.PlaySpatialSoundEffect(clip, collision.contacts[0].point, colliderPitchOffset, randomisePitch);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (hasSoundEffects[1] && (includeGround || !other.gameObject.CompareTag("Level"))) {
            AudioClip clip = triggerSoundEffects[Random.Range(0, triggerSoundEffects.Length)];
            GameManager.audioManager.PlaySpatialSoundEffect(clip, transform.position, triggerPitchOffset, randomisePitch);
        }
    }
}