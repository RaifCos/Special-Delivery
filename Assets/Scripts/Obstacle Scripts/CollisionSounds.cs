using UnityEngine;

// Script to handle the sounds emitted by Obstacles when they collide with another object.
public class CollisionSounds : MonoBehaviour {
    [SerializeField] private AudioClip[] collisionSoundEffects;
    [SerializeField] private AudioClip[] triggerSoundEffects;
    [SerializeField] private bool randomisePitch; 
    private readonly bool[] hasSoundEffects = new bool[2];

    private void Start() {
        hasSoundEffects[0] = collisionSoundEffects.Length > 0;
        hasSoundEffects[1] = triggerSoundEffects.Length > 0;
    }

    private void OnCollisionEnter(Collision collision) {
        if (hasSoundEffects[0]) {
            AudioClip clip = collisionSoundEffects[Random.Range(0, collisionSoundEffects.Length)];
            GameManager.audioManager.PlaySoundEffect(clip, randomisePitch);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (hasSoundEffects[1]) {
            AudioClip clip = triggerSoundEffects[Random.Range(0, triggerSoundEffects.Length)];
            GameManager.audioManager.PlaySoundEffect(clip, randomisePitch);
        }
    }
}