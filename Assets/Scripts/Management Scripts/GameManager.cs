using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Script to handle main game functionality.
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour {
    private static readonly WaitForSeconds _waitForSeconds1 = new(1f);
    
    [Header("Other Manager Scripts")]
    public static GameManager instance;
    public static OpeningMenuManager openingMenuManager;
    public static MainMenuManager mainMenuManager;
    public static GarageMenuManager garageMenuManager;
    public static AchievementMenuManager achievementMenuManager;
    public static GalleryManager galleryManager;
    public static UnlockCutsceneManager unlockCutsceneManager;
    public static SettingsManager settingsManager;
    public static GameplayManager gameplayManager;
    public static DeliveryManager deliveryManager;
    public static ObstacleManager obstacleManager;
    public static AudioManager audioManager;
    public static NewsTextScroller newsTextScroller;
    public static DataManager dataManager;

    [Header("Player Preferences")]
    private int saveFile; 
    private static int difficulty;
    private float musicVolume;
    private float soundEffectVolume;
    private bool qualityShadows;
    private int controlScheme; // 0 - Buttons : 1 - Joystick

    [Header("Universal Variables")]
    [SerializeField] private Material paletteMaterial;

    void Awake() { 
        instance = this;
        saveFile = PlayerPrefs.GetInt("SaveFile", 0);
        musicVolume = PlayerPrefs.GetFloat("VolumeMusic_" + saveFile, 0.85f);
        soundEffectVolume = PlayerPrefs.GetFloat("VolumeEffects_" + saveFile, 0.85f);
        controlScheme = PlayerPrefs.GetInt("ControllerScheme_" + saveFile, 0);
        ToggleShadows(PlayerPrefs.GetInt("Shadows_" + saveFile, 0) == 0);

        // Disable Mouse input 
        Cursor.visible = false;
        InputSystem.DisableDevice(Mouse.current);
    }
    
    // Getter Method for the current difficulty. 
    public int GetDifficulty() { return difficulty; }

    // Setter Method for the current difficulty. 
    public void SetDifficulty(int input) { difficulty = input; }

    public void SetMusicVolume(float input) {
        musicVolume = input;
        PlayerPrefs.SetFloat("VolumeMusic_" + saveFile, musicVolume);
        PlayerPrefs.Save();
    }

    public float GetMusicVolume() => musicVolume; 

    public void SetSoundEffectVolume(float input) {
        soundEffectVolume = input;
        PlayerPrefs.SetFloat("VolumeEffects_" + saveFile, soundEffectVolume);
        PlayerPrefs.Save();
    }

    public float GetSoundEffectVolume() => soundEffectVolume;

    // 0 - High Quality Shadows
    // 1 - Low Quality Shadows
    public void ToggleShadows(bool input) {
        qualityShadows = input;
        int res = qualityShadows? 0: 1;
        PlayerPrefs.SetInt("Shadows_" + saveFile, res);
        PlayerPrefs.Save();
    }

    public bool GetShadowQuality() { return qualityShadows; }

    public void SetControllerScheme(int input) {
        controlScheme = input;
        PlayerPrefs.SetInt("ControllerScheme_" + saveFile, input);
    }

    public int GetControllerScheme() => controlScheme;     

    public void ResetPlayerPrefs() {
        SetMusicVolume(0.85f);
        SetSoundEffectVolume(0.85f);
        ToggleShadows(true);
        SetControllerScheme(0);
    }
    
    public IEnumerator LoadAsyncScene(string scene, bool save = true) {
        Instantiate(Resources.Load<GameObject>("UI/LoadingScreen"));
        if (save) dataManager.SaveData();
        audioManager.StopGameMusic();
        yield return _waitForSeconds1;
        if (scene == "MainMenu" && dataManager.CutscenesQueued()) scene = "UnlockScene";
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
        while (!asyncLoad.isDone) { yield return null; }
    }

    public void ResetCurrentButton() {
        GameObject currentButton = EventSystem.current.currentSelectedGameObject;
        if (currentButton == null) return;
        Transform transform = currentButton.transform;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public int GetSaveFile() => saveFile;

    public void SetSaveFile(int input) {
        saveFile = input;
        PlayerPrefs.SetInt("SaveFile", saveFile);
        PlayerPrefs.Save();
    }

    public Material GetPalette() => paletteMaterial;
}