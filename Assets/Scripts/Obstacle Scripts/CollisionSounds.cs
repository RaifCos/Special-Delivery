using UnityEngine;

// Script to handle the sounds emitted by Obstacles when they collide with another object.
public class CollisionSounds : MonoBehaviour {
    public AudioClip[] soundEffects;
    public AudioClip[] GetSoundArray() => soundEffects;
}