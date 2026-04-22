using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Script to handle main game functionality.
public class GameManager : MonoBehaviour {
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
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

    private EventSystem eventSystem;

    [Header("Music Settings")]
    public GameObject muteButton, unmuteButton;

    [Header("Player Preferences")]
    private int saveFile; 
    private static int difficulty;
    private bool isMusicPlaying, qualityShadows;

    void Awake() { 
        eventSystem = EventSystem.current;
        instance = this;
        saveFile = PlayerPrefs.GetInt("SaveFile", 0);
    }

    // Start is called before the first frame update.
    void Start() { 
        ToggleMusic(PlayerPrefs.GetInt("MuteOn_" + saveFile, 0) == 0);
        ToggleShadows(PlayerPrefs.GetInt("Shadows_" + saveFile, 0) == 0);
    }

    // Getter Method for the current difficulty. 
    public int GetDifficulty() { return difficulty; }

    // Setter Method for the current difficulty. 
    public void SetDifficulty(int input) { difficulty = input; }
    public void ToggleMusic(bool isOn) {
        unmuteButton.SetActive(!isOn);
        muteButton.SetActive(isOn);
        if(isOn) {
            eventSystem.SetSelectedGameObject(muteButton);
        } else {
            eventSystem.SetSelectedGameObject(unmuteButton);
        }
        isMusicPlaying = isOn;
        audioManager.ToggleMusic(isOn);
        int res = isOn? 0: 1;
        PlayerPrefs.SetInt("MuteOn_" + saveFile, res);
        PlayerPrefs.Save();
    }

    public void ToggleShadows(bool areLower) {
        qualityShadows = !areLower;
        int res = qualityShadows? 0: 1;
        PlayerPrefs.SetInt("Shadows_" + saveFile, res);
        PlayerPrefs.Save();
    }

    public bool GetMusicPlaying() { return isMusicPlaying; }

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
}