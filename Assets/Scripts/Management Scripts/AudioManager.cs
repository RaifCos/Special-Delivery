using System.Collections;
using UnityEngine;

// Script to handle Audio not in the game world (Music and Fanfare)
public class AudioManager : MonoBehaviour
{
    public AudioSource music, parcelSound, soundscape;
    public AudioClip musicStart, musicLoop, musicEnd;
    public AudioClip soundParcel, soundSpot;
    private bool isPlaying = false;

    void Awake() {
        GameManager.audioManager = this;
    }

    public void Start() {
        // Preload all Audio Sounds to prevent gaps or delays when they're needed.
        musicStart.LoadAudioData();
        musicLoop.LoadAudioData();
        musicEnd.LoadAudioData();
        soundParcel.LoadAudioData();
        soundSpot.LoadAudioData();
    }

    // Coroutine to play music during gameplay.
    public IEnumerator StartGameMusic() {
        music.volume = 0.85f;
        // Play the "start" clip once.
        isPlaying = true;
        music.loop = false;
        music.clip = musicStart;
        music.Play();
        yield return new WaitUntil(() => !music.isPlaying);
        // If the game is still in session after the "start" clip finishes, then loop the "loop" clip.
        if (isPlaying) {
            music.loop = true;
            music.clip = musicLoop;
            music.Play();
        }
    }

    // Function to stop the main game music loop.
    public void StopGameMusic() {
        StopCoroutine(StartGameMusic());
        isPlaying = false;
        music.loop = false;
        music.Stop(); 
    }

    // Coroutine to play the music on the game over screen.
    public IEnumerator EndGameMusic() {
        StopGameMusic();
        soundscape.Stop();
        music.clip = musicEnd;
        music.Play();
        yield return new WaitUntil(() => !music.isPlaying);
    }

    // Function to play the fanfare when a Parcel/Delivery Spot is reached.
    public void PlayParcelSound(bool isParcel) {
        if (isParcel) { parcelSound.clip = soundParcel; }
        else { parcelSound.clip = soundSpot; }
        parcelSound.Play();
    }

    public void ToggleMusic(bool isPlaying) {
        music.mute = !isPlaying;
    }

    // Function to adjust music volume when moving between gameplay and the pause menu. 
    public void TogglePause(bool isPaused) {
        if (isPaused) { music.volume = 0.3f; } // Decrease volume when paused.
        else { music.volume = 0.85f; } // Increase volume when exiting the pause menu.
    }

}
