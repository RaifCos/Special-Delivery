using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Script to handle Audio not in the game world (Music and Fanfare)
public class AudioManager : MonoBehaviour {
    [Header("Main Audio Sources")] 
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioSource soundscape;

    [Header("Sound Effect Audio Sources")] 
    [SerializeField] private AudioSource soundEffectSource;
    [SerializeField] private GameObject[] spatialAudioSources;

    [Header("Music Audio Clips")] 
    [SerializeField] private AudioClip musicStart;
    [SerializeField] private AudioClip musicLoop;
    [SerializeField] private AudioClip musicEnd;

    [Header("Other Variables")]
    [SerializeField] private AudioClip soundParcel;
    [SerializeField] private AudioClip soundSpot;
    [SerializeField] private AudioClip defaultCrashSound;
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

    #region Music

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

    public void StopGameMusic() {
        if (gameMusicCoroutine != null) StopCoroutine(gameMusicCoroutine);
        isPlaying = false;
        music.loop = false;
        music.Stop(); 
    }

    public IEnumerator EndGameMusic() {
        StopGameMusic();
        soundscape.Stop();
        music.clip = musicEnd;
        music.Play();
        yield return new WaitUntil(() => !music.isPlaying);
    }

    #endregion

    #region Sound Effects

    public void PlayParcelSound(bool isParcel) => PlaySoundEffect(isParcel? soundParcel: soundSpot); 

    public void DefaultCrashSound(Vector3 position) => PlaySpatialSoundEffect(defaultCrashSound, position, 0f, true);

    public void PlaySoundEffect(AudioClip sound) {
        soundEffectSource.clip = sound;
        soundEffectSource.Play();
    }

    public void PlaySpatialSoundEffect(AudioClip sound, Vector3 position, float pitchOffset, bool randomisePitch) {
        GameObject chosenSoundObject = null;
        AudioSource chosenSoundSource = null;
        
        // Check for Object in the Soundbank that isn't playing.
        foreach(var spatialObj in spatialAudioSources) {
            AudioSource spatialSource = spatialObj.GetComponent<AudioSource>();
            if (!spatialSource.isPlaying) {
                chosenSoundObject = spatialObj;
                chosenSoundSource = spatialSource;
                break;
            }
        }

        // Play Sound Effect.
        if (chosenSoundObject != null) {
            chosenSoundObject.transform.position = position;
            chosenSoundSource.clip = sound;
            chosenSoundSource.pitch = randomisePitch? Random.Range(0.8f, 1.1f) + pitchOffset: 1f + pitchOffset;
            chosenSoundSource.Play();
        }
    }

    #endregion

    #region Volume Settings
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
    
    #endregion
}
