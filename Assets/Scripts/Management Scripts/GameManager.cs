using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Script to handle main game functionality.
public class GameManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    [Header("Other Manager Scripts")]
    public static GameManager instance;
    public static OpeningMenuManager openingMenuManager;
    public static MainMenuManager mainMenuManager;
    public static ShopManager shopManager;
    public static AchievementManager achievementManager;
    public static GalleryManager galleryManager;
    public static GameplayManager gameplayManager;
    public static DeliveryManager deliveryManager;
    public static ObstacleManager obstacleManager;
    public static ObstacleData obstacleData;
    public static ShopData shopData;
    public static AudioManager audioManager;
    public static NewsTextScroller newsTextScroller;

    private static int bestScore, difficulty, money;
    private bool isShopUnlocked;

    [Header("Music Settings")]
    public GameObject muteButton;
    public GameObject unmuteButton;
    private bool isMusicPlaying;

    void Awake() { instance = this; }

    // Start is called before the first frame update.
    void Start() {
        SetShopProgress(PlayerPrefs.GetInt("ShopUnlocked", 0) == 1);
        SetMoney(PlayerPrefs.GetInt("Money", 1000000));
        ToggleMusic(PlayerPrefs.GetInt("MuteOn", 0) == 0);
    }

    public int GetBestScore() { return bestScore = PlayerPrefs.GetInt("HighScore", 0); }

    // Setter Method for the player's high score, also updates the UI and saves the new data.
    public void SetBestScore(int val) {
        bestScore = val;
        PlayerPrefs.SetInt("HighScore", bestScore);
        PlayerPrefs.Save();
    }

    // Getter Method for the current difficulty. 
    public int GetDifficulty() { return difficulty; }

    // Setter Method for the current difficulty. 
    public void SetDifficulty(int input) { difficulty = input; }

    public int GetMoney() { return money; }

    public void SetMoney(int input) { 
        money = input;
        PlayerPrefs.SetInt("Money", input);
        PlayerPrefs.Save();
    }

    public bool MoneyTransaction(int amount) {
        if (amount < 0 && Math.Abs(amount) > money) { return false; }
        else { SetMoney(money + amount); return true; }
    }

    public bool GetShopProgress() { return isShopUnlocked; }

    public void SetShopProgress(bool input) { 
        isShopUnlocked = input;
        int res = input? 1: 0;
        PlayerPrefs.SetInt("shopOpened", res);
        PlayerPrefs.Save();
    }

    public void ToggleMusic(bool isOn) {
        unmuteButton.SetActive(!isOn);
        muteButton.SetActive(isOn);
        isMusicPlaying = isOn;
        audioManager.ToggleMusic(isOn);
        int res = isOn? 0: 1;
        PlayerPrefs.SetInt("MuteOn", res);
        PlayerPrefs.Save();
    }

    public bool GetMusicPlaying() { return isMusicPlaying; }
    
    public IEnumerator LoadAsyncScene(string scene) {
        audioManager.StopGameMusic();
        yield return _waitForSeconds1;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
        while (!asyncLoad.isDone) { yield return null; }
    }
}