using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Script to handle Audio not in the game world (Music and Fanfare)
public class AudioManager : MonoBehaviour {
    [Header("Main Audio Sources")] 
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioSource soundscape;
    [SerializeField] private float musicStartDelay = 0;

    [Header("Sound Effect Audio Sources")] 
    [SerializeField] private AudioSource soundEffectSource;
    [SerializeField] private GameObject[] spatialAudioSourceObjects;
    private AudioSource[] spatialAudioSources;
    private SoundEffectVolume[] soundEffectVolumeControllers;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip soundParcel;
    [SerializeField] private AudioClip soundSpot;
    [SerializeField] private AudioClip soundBossParcel;
    [SerializeField] private AudioClip soundBossSpotPlayer;
    [SerializeField] private AudioClip soundBossSpotBoss;
    [SerializeField] private AudioClip defaultCrashSound;
    [SerializeField] private AudioClip soundButtonEnter;
    [SerializeField] private AudioClip soundButtonBack;
    [SerializeField] private AudioClip soundButtonLocked;

    [Header("Volume Settings")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundEffectVolumeSlider;
    private float volumeMusic, volumeSoundEffects;

    private AudioClip musicStart, musicLoop, musicEnd, musicEndAlternate;
    private Coroutine gameMusicCoroutine;
    private bool isPlaying, isPaused;

    void Awake() {
        GameManager.audioManager = this;
        int len = spatialAudioSourceObjects.Length;
        spatialAudioSources = new AudioSource[len];
        for (int i = 0; i < len; i++) {
            spatialAudioSources[i] = spatialAudioSourceObjects[i].GetComponent<AudioSource>();
        } soundEffectVolumeControllers = Resources.FindObjectsOfTypeAll<SoundEffectVolume>();
    }

    public void Initalize(AudioClip startClip, AudioClip loopClip) {
        musicStart = startClip;
        musicLoop = loopClip;

        // Preload all Audio Sounds to prevent gaps or delays when they're needed.
        musicStart.LoadAudioData();
        musicLoop.LoadAudioData();
        soundParcel.LoadAudioData();
        soundSpot.LoadAudioData();

        soundButtonEnter.LoadAudioData();
        soundButtonBack.LoadAudioData();
        soundButtonLocked.LoadAudioData();

        gameMusicCoroutine = StartCoroutine(GameMusicLoop());
    }

    public void Initalize(AudioClip startClip, AudioClip loopClip, AudioClip endClip) {
        musicStart = startClip;
        musicLoop = loopClip;
        musicEnd = endClip;

        // Preload all Audio Sounds to prevent gaps or delays when they're needed.
        musicStart.LoadAudioData();
        musicLoop.LoadAudioData();
        musicEnd.LoadAudioData();
        soundParcel.LoadAudioData();
        soundSpot.LoadAudioData();

        soundButtonEnter.LoadAudioData();
        soundButtonBack.LoadAudioData();
        soundButtonLocked.LoadAudioData();

        gameMusicCoroutine = StartCoroutine(GameMusicLoop());
    }

    public void Initalize(AudioClip startClip, AudioClip loopClip, AudioClip endClipA, AudioClip endClipB) {
        musicStart = startClip;
        musicLoop = loopClip;
        musicEnd = endClipA;
        musicEndAlternate = endClipB;

        // Preload all Audio Sounds to prevent gaps or delays when they're needed.
        musicStart.LoadAudioData();
        musicLoop.LoadAudioData();
        musicEnd.LoadAudioData();
        soundParcel.LoadAudioData();
        soundSpot.LoadAudioData();

        soundButtonEnter.LoadAudioData();
        soundButtonBack.LoadAudioData();
        soundButtonLocked.LoadAudioData();
        
        gameMusicCoroutine = StartCoroutine(GameMusicLoop());
    }

    #region Music

    // Coroutine to play music during gameplay.
    public IEnumerator GameMusicLoop() {
        if (musicStartDelay != 0) yield return new WaitForSeconds(musicStartDelay);

        // Apple Initial Volume Values 
        volumeMusic = GameManager.instance.GetMusicVolume();
        volumeSoundEffects = GameManager.instance.GetSoundEffectVolume();
        music.volume = volumeMusic;
        ConfirmSoundEffectVolumeChange();

        // Have Volume Sliders match current values (without triggering Value Change functions)
        if (musicVolumeSlider != null) { musicVolumeSlider.SetValueWithoutNotify(volumeMusic * 20f); }
        if (soundEffectVolumeSlider != null) { soundEffectVolumeSlider.SetValueWithoutNotify(volumeSoundEffects * 20f); }

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
        music.clip = musicEnd;
        music.Play();
        yield return new WaitUntil(() => !music.isPlaying);
    }

    public IEnumerator EndBossMusic(int winner) {
        StopGameMusic();
        soundscape.Stop();
        SetMusicPitch(1f);
        music.clip = winner == 0 ? musicEnd : musicEndAlternate;
        music.Play();
        yield return new WaitUntil(() => !music.isPlaying);
    }

    public IEnumerator UnlockFanfare(AudioClip fanfare) {
        SetMusicPitch(1f);
        music.clip = fanfare;
        volumeMusic = GameManager.instance.GetMusicVolume();
        music.volume = volumeMusic;
        music.Play();
        yield return new WaitUntil(() => !music.isPlaying);
    }

    public bool IsMusicPlaying() => music.isPlaying;

    public void SetMusicPitch(float input) => music.pitch = input;

    #endregion

    #region Sound Effects

    public void PlaySoundEffect(AudioClip sound) {
        soundEffectSource.pitch = 1f;
        soundEffectSource.clip = sound;
        soundEffectSource.Play();
    }

    public void PlaySoundEffect(AudioClip sound, float pitch) {
        soundEffectSource.pitch = pitch;
        soundEffectSource.clip = sound;
        soundEffectSource.Play();
    }

    public void PlayButtonSound(int state, bool isMainMenu) {
        AudioClip clip = null;

        switch (state) {
            case 0: clip = soundButtonEnter; break;
            case 1: clip = soundButtonBack; break;
            case 2: clip = soundButtonLocked; break;
        } 

        if (isMainMenu) PlaySoundEffect(clip, 0.8909f);
        else PlaySoundEffect(clip);
    }

    public void PlayParcelSound(bool isParcel) => PlaySoundEffect(isParcel? soundParcel: soundSpot); 

    public void PlayBossParcelSound(bool isParcel, bool isPlayer) {
        if (isParcel) { PlaySoundEffect(soundBossParcel); }
        else { PlaySoundEffect(isPlayer ? soundBossSpotPlayer : soundBossSpotBoss); }
    }

    public void PlaySpatialSoundEffect(AudioClip sound, Vector3 position, bool priority) {
        int chosen = -1;

        // Check for Object in the Soundbank that isn't playing.
        for (int i = 0; i < spatialAudioSourceObjects.Length; i++) {
            if (!spatialAudioSources[i].isPlaying) {
                chosen = i;
                break;
            } 
        }

        // If there is no capacity in the Soundbank but the current sound takes priority, chose a random sound to overwrite.
        if (chosen == -1) {
            if (!priority) return;
            else chosen = Random.Range(0, spatialAudioSources.Length);
        }

        // Set Position to play Sound Effect.
        spatialAudioSourceObjects[chosen].transform.position = position;
        
        // Play Sound Effect.
        AudioSource chosenSoundSource = spatialAudioSources[chosen];
        chosenSoundSource.clip = sound;
        chosenSoundSource.pitch = 1f;
        chosenSoundSource.Play();
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
    
    public void DefaultCrashSound(Vector3 position) => PlaySpatialSoundEffect(defaultCrashSound, position, 0f, true);

    #endregion

    #region Volume Settings
    public void AdjustMusicVolume() { 
        volumeMusic = musicVolumeSlider.value / 20f; 
        if (isPaused) { music.volume = volumeMusic/2; }
        else { music.volume = volumeMusic; }
    }

    public void ConfirmMusicVolumeChange() => GameManager.instance.SetMusicVolume(volumeMusic);

    public void TogglePause(bool paused) { 
        isPaused = paused;
        if (isPaused) { music.volume = volumeMusic/2; }
        else { music.volume = volumeMusic; }
    }

    public void AdjustSoundEffectVolume() {
        volumeSoundEffects = soundEffectVolumeSlider.value / 20f;
        soundEffectSource.volume = volumeSoundEffects;
        PlayParcelSound(true);
    }

    public void ConfirmSoundEffectVolumeChange() {
        GameManager.instance.SetSoundEffectVolume(volumeSoundEffects);
        foreach (SoundEffectVolume sev in soundEffectVolumeControllers) { sev.AdjustVolume(volumeSoundEffects); }
    }
    
    #endregion
}
