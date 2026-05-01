using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Script to handle Audio not in the game world (Music and Fanfare)
public class AudioManager : MonoBehaviour {
    public AudioSource music, effectSound, soundscape;
    public AudioClip musicStart, musicLoop, musicEnd;
    public AudioClip soundParcel, soundSpot;
    [SerializeField] private Slider volumeSlider;
    private Coroutine gameMusicCoroutine;
    private float volume;
    private bool isPlaying, isPaused;

    void Awake() => GameManager.audioManager = this;

    public void Start() {
        // Preload all Audio Sounds to prevent gaps or delays when they're needed.
        musicStart.LoadAudioData();
        musicLoop.LoadAudioData();
        musicEnd.LoadAudioData();
        soundParcel.LoadAudioData();
        soundSpot.LoadAudioData();
    }

    // Coroutine to play music during gameplay.
    public IEnumerator GameMusicLoop() {
        volume = GameManager.instance.GetMusicVolume();
        music.volume = volume;
        if(volumeSlider != null) { volumeSlider.value = volume;}
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

    public void StartGameMusic() => gameMusicCoroutine = StartCoroutine(GameMusicLoop());

    // Function to stop the main game music loop.
    public void StopGameMusic() {
        if (gameMusicCoroutine != null) StopCoroutine(gameMusicCoroutine);
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
        if (isParcel) { effectSound.clip = soundParcel; }
        else { effectSound.clip = soundSpot; }
        effectSound.Play();
    }

    public void SetEffectSound(AudioClip sound) { effectSound.clip = sound; }

    public void PlayEffectSound() { effectSound.Play(); }

    public void AdjustVolume() { 
        volume = volumeSlider.value; 
        if (isPaused) { music.volume = volume/2; }
        else { music.volume = volume; }
    }

    public void ConfirmVolumeChange() => GameManager.instance.SetMusicVolume(volume);

    public void TogglePause(bool paused) { 
        isPaused = paused;
        if (isPaused) { music.volume = volume/2; }
        else { music.volume = volume; }
    }

}
