using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Script to handle main game functionality.
public class MainMenuManager : MonoBehaviour {
    public GameObject menuUI, achievementUI, galleryUI, creditsUI;

    [Header("Achievement Menu Variables")]
    public GameObject buttonIcons;
    public GameObject achievementDisplay;
    public Sprite lockedSprite;
    public Sprite[] achievementSprite;

    void Awake() { GameManager.mainMenuManager = this; }

    public void Start() {
        GameManager.achievementManager.UpdateData(true);
        StartCoroutine(GameManager.audioManager.StartGameMusic());
    }

    public void StartGame(int difficulty) {
        GameManager.instance.SetDifficulty(difficulty);
        AlternateMainMenus(2);
        StartCoroutine(GameManager.instance.LoadAsyncScene("City"));
    }

    // Function to alterante between the UI Menus.
    public void AlternateMainMenus(int menu) {
        switch (menu) {
            case 0: { // Main Menu
                menuUI.SetActive(true);
                galleryUI.SetActive(false);
                achievementUI.SetActive(false);
                break; }
            case 1: { // Achievement Menu
                menuUI.SetActive(false);
                achievementUI.SetActive(true);
                achievementUI.transform.GetChild(4).gameObject.GetComponent<TMP_Text>().text = "HIGH-SCORE: " + GameManager.instance.GetBestScore().ToString();
                DisplayAchievement(0);
                break; }
            case 2: { // Gallery
                menuUI.SetActive(false);
                galleryUI.SetActive(true);
                GameManager.galleryManager.UpdateGalleryUI();
                GameManager.galleryManager.AlternateGalleryMenus(true);
                break; }
            case 3: { // Loading Screen
                menuUI.SetActive(false);
                Instantiate(Resources.Load<GameObject>("LoadingScreen"));
                break; }
        }  
    }

    // Function to update the UI in the Achievement Menu based on the Achievement's state.
    public void UpdateAchievementUI(int id) {
        Image img = buttonIcons.transform.GetChild(id).GetComponent<Image>();
        if (GameManager.achievementManager.GetAchievement(id).achieved) { img.sprite = achievementSprite[id]; } // Achievement is unlocked.
        else { img.sprite = lockedSprite; } // Achievement is locked.
    }
    
    public void DisplayAchievement(int id) {
        Image img = buttonIcons.transform.GetChild(id).GetComponent<Image>();
        achievementDisplay.transform.GetChild(0).GetComponent<Image>().sprite = img.sprite;
        // Achievement is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            achievementDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = "???";
        } else { // Achievement is unlocked, so show achievement information.
            achievementDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = GameManager.achievementManager.GetAchievement(id).name;
        } 
        
        // For certain achievements, display the associated tracking variable for clarity.
        string res = GameManager.achievementManager.GetAchievement(id).description;
        switch (id) {
            case 1: { // Lifetime Deliveries
                    res += " [" + GameManager.achievementManager.GetLifetimeScore() + "]";
                    break; }
            case 3:
            case 4: { // Player Crashes
                    res += " [" +  GameManager.achievementManager.GetPlayerCrashes() + "]";
                    break; }}
        achievementDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = res;
    }  
}