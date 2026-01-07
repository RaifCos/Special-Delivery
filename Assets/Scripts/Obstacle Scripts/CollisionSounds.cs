using UnityEngine;

// Script to handle the sounds emitted by Obstacles when they collide with another object.
public class CollisionSounds : MonoBehaviour
{
    public AudioSource sound;
    public AudioClip[] soundEffects;
    public bool usesCollider, includeGround;

    // usesCollider indicates if collisions with the Obstacle's collider should be included.
    // includeGround indicates if collisions with the Ground should count.

    private void OnCollisionEnter(Collision collision) {
        if ((!collision.gameObject.CompareTag("Level") || includeGround) && usesCollider && !sound.isPlaying) {
            PlayRandomSound(); }
    }

    private void OnTriggerEnter(Collider other) {
        if ((!other.gameObject.CompareTag("Level") || includeGround) && !other.gameObject.CompareTag("Parcel") && !sound.isPlaying) {
            PlayRandomSound(); }
    }

    // Function to play a random sound from the array of choices.
    private void PlayRandomSound() {
        sound.clip = soundEffects[Random.Range(0, soundEffects.Length)];
        sound.Play();
    }

}