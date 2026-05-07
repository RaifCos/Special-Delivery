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
    [SerializeField] private GameObject[] spatialAudioSourceObjects;
    private AudioSource[] spatialAudioSources;

    [Header("Music Audio Clips")] 
    [SerializeField] private AudioClip standardStart;
    [SerializeField] private AudioClip stnadardLoop;
    [SerializeField] private AudioClip standardEnd;
    [SerializeField] private AudioClip bossStart;
    [SerializeField] private AudioClip bossLoop;
    [SerializeField] private AudioClip bossWin;
    [SerializeField] private AudioClip bossLose;
    private AudioClip musicStart, musicLoop;

    [Header("Other Variables")]
    [SerializeField] private AudioClip soundParcel;
    [SerializeField] private AudioClip soundSpot;
    [SerializeField] private AudioClip soundBossParcel;
    [SerializeField] private AudioClip soundBossSpotPlayer;
    [SerializeField] private AudioClip soundBossSpotBoss;
    [SerializeField] private AudioClip defaultCrashSound;
    [SerializeField] private float soundscapeVolume; 
    [SerializeField] private Slider volumeSlider;
    private Coroutine gameMusicCoroutine;
    private float volume;
    private bool isPlaying, isPaused;

    void Awake() {
        GameManager.audioManager = this;
        int len = spatialAudioSourceObjects.Length;
        spatialAudioSources = new AudioSource[len];
        for (int i = 0; i < len; i++) {
            spatialAudioSources[i] = spatialAudioSourceObjects[i].GetComponent<AudioSource>();
        }
    }

    public void Start() {
        if (GameManager.instance.GetDifficulty() == 2) {
            musicStart = bossStart;
            musicLoop = bossLoop;
        } else {
            musicStart = standardStart;
            musicLoop = stnadardLoop;
        }

        // Preload all Audio Sounds to prevent gaps or delays when they're needed.
        musicStart.LoadAudioData();
        musicLoop.LoadAudioData();
        soundParcel.LoadAudioData();
        soundSpot.LoadAudioData();
        gameMusicCoroutine = StartCoroutine(GameMusicLoop());
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

    public void StopGameMusic() {
        if (gameMusicCoroutine != null) StopCoroutine(gameMusicCoroutine);
        isPlaying = false;
        music.loop = false;
        music.Stop(); 
    }

    public IEnumerator EndGameMusic() {
        StopGameMusic();
        soundscape.Stop();
        SetMusicPitch(1f);
        music.clip = standardEnd;
        music.Play();
        yield return new WaitUntil(() => !music.isPlaying);
    }

    public IEnumerator EndBossMusic(int winner) {
        StopGameMusic();
        soundscape.Stop();
        SetMusicPitch(1f);
        music.clip = winner == 0 ? bossWin : bossLose;
        music.Play();
        yield return new WaitUntil(() => !music.isPlaying);
    }

    public void SetMusicPitch(float input) => music.pitch = input;

    #endregion

    #region Sound Effects

    public void PlayParcelSound(bool isParcel) => PlaySoundEffect(isParcel? soundParcel: soundSpot); 

    public void PlayBossParcelSound(bool isParcel, bool isPlayer) {
        if (isParcel) { PlaySoundEffect(soundBossParcel); }
        else { PlaySoundEffect(isPlayer ? soundBossSpotPlayer : soundBossSpotBoss); }
    }

    public void DefaultCrashSound(Vector3 position) => PlaySpatialSoundEffect(defaultCrashSound, position, 0f, true);

    public void PlaySoundEffect(AudioClip sound) {
        soundEffectSource.clip = sound;
        soundEffectSource.Play();
    }

    public void PlaySpatialSoundEffect(AudioClip sound, Vector3 position, float pitchOffset, bool randomisePitch) {
        int chosen = -1;

        // Check for Object in the Soundbank that isn't playing.
        for (int i = 0; i < spatialAudioSourceObjects.Length; i++) {
            if (!spatialAudioSources[i].isPlaying) {
                chosen = i;
                break;
            } 
        }

        if (chosen == -1) return;

        // Set Position to play Sound Effect.
        spatialAudioSourceObjects[chosen].transform.position = position;
        
        // Play Sound Effect.
        AudioSource chosenSoundSource = spatialAudioSources[chosen];
        chosenSoundSource.clip = sound;
        chosenSoundSource.pitch = randomisePitch? Random.Range(0.8f, 1.1f) + pitchOffset: 1f + pitchOffset;
        chosenSoundSource.Play();
        
    }

    #endregion

    #region Volume Settings
    public void AdjustMusicVolume() { 
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
