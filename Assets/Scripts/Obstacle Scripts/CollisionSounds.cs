using UnityEngine;

// Script to handle the sounds emitted by Obstacles when they collide with another object.
public class CollisionSounds : MonoBehaviour {
    [SerializeField] private AudioClip[] collisionSoundEffects;
    [SerializeField] private AudioClip[] triggerSoundEffects;
    [SerializeField] private bool includeGround; 
    [SerializeField] private float colliderPitchOffset; 
    [SerializeField] private float triggerPitchOffset; 
    [SerializeField] private bool randomisePitch;
    [SerializeField] private float soundCooldown;
    private readonly bool[] hasSoundEffects = new bool[2];
    private float lastCollisionTime = -Mathf.Infinity;
    private float lastTriggerTime = -Mathf.Infinity;

    private void Start() {
        hasSoundEffects[0] = collisionSoundEffects.Length > 0;
        hasSoundEffects[1] = triggerSoundEffects.Length > 0;
    }

    private void OnCollisionEnter(Collision collision) {
        if (!hasSoundEffects[0]) return;
        if (!includeGround && collision.gameObject.CompareTag("Level")) return;
        if (Time.time - lastCollisionTime < soundCooldown) return;

        lastCollisionTime = Time.time;
        AudioClip clip = collisionSoundEffects[Random.Range(0, collisionSoundEffects.Length)];
        GameManager.audioManager.PlaySpatialSoundEffect(clip, collision.contacts[0].point, colliderPitchOffset, randomisePitch);
    }

    private void OnTriggerEnter(Collider other) {
        if (!hasSoundEffects[1]) return;
        if (!includeGround && other.gameObject.CompareTag("Level")) return;
        if (Time.time - lastTriggerTime < soundCooldown) return;

        lastTriggerTime = Time.time;
        AudioClip clip = triggerSoundEffects[Random.Range(0, triggerSoundEffects.Length)];
        GameManager.audioManager.PlaySpatialSoundEffect(clip, transform.position, triggerPitchOffset, randomisePitch);
    }
}