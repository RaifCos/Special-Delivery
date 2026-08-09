using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Script to handle main game functionality.
public class GameManager : MonoBehaviour {
    private static readonly WaitForSeconds _waitForSeconds1 = new(1f);
    
    [Header("Other Manager Scripts")]
    public static GameManager instance;
    public static OpeningMenuManager openingMenuManager;
    public static MainMenuManager mainMenuManager;
    public static GarageMenuManager garageMenuManager;
    public static AchievementMenuManager achievementMenuManager;
    public static GalleryManager galleryManager;
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
    private static Level_SO currentLevel;
    private float musicVolume;
    private bool qualityShadows;

    [Header("Universal Variables")]
    [SerializeField] private Material paletteMaterial;

    void Awake() { 
        instance = this;
        saveFile = PlayerPrefs.GetInt("SaveFile", 0);
        musicVolume = PlayerPrefs.GetFloat("VolumeMusic_" + saveFile, 0.85f);
        ToggleShadows(PlayerPrefs.GetInt("Shadows_" + saveFile, 0) == 0);
    }
    
    // Getter Method for the current difficulty. 
    public int GetDifficulty() { return difficulty; }

    // Setter Method for the current difficulty. 
    public void SetDifficulty(int input) { difficulty = input; }

    public Level_SO GetCurrentLevel() => currentLevel;
    
    public void SetCurrentLevel(Level_SO level) => currentLevel = level;

    public void SetMusicVolume(float input) {
        musicVolume = input;
        PlayerPrefs.SetFloat("VolumeMusic_" + saveFile, musicVolume);
        PlayerPrefs.Save();
    }

    public float GetMusicVolume() => musicVolume; 

    public void ToggleShadows(bool input) {
        qualityShadows = input;
        int res = qualityShadows? 0: 1;
        PlayerPrefs.SetInt("Shadows_" + saveFile, res);
        PlayerPrefs.Save();
    }

    public bool GetShadowQuality() { return qualityShadows; }
    
    public IEnumerator LoadAsyncScene(string scene) {
        audioManager.StopGameMusic();
        yield return _waitForSeconds1;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
        while (!asyncLoad.isDone) { yield return null; }
    }

    public int GetSaveFile() => saveFile;

    public void SetSaveFile(int input) {
        saveFile = input;
        PlayerPrefs.SetInt("SaveFile", saveFile);
        PlayerPrefs.Save();
    }

    public Material GetPalette() => paletteMaterial;
}