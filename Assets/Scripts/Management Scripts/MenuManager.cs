using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Script to handle main game functionality.
public class MenuManager : MonoBehaviour {
    public GameObject menuUI, achievementUI, settingsUI, confirmUI, galleryUI, creditsUI;
    private static int confirmationUIID;

    [Header("Achievement Menu Variables")]
    public GameObject buttonIcons;
    public GameObject achievementDisplay;
    public Sprite lockedSprite;
    public Sprite[] achievementSprite;

    void Awake() { GameManager.menuManager = this; }

    public void Start() {
        achievementUI.transform.GetChild(5).gameObject.GetComponent<TMP_Text>().text = "HIGH-SCORE: " + GameManager.instance.GetBestScore().ToString();
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
                settingsUI.SetActive(false);
                break; }
            case 1: { // Achievement Menu
                menuUI.SetActive(false);
                achievementUI.SetActive(true);
                achievementUI.transform.GetChild(5).gameObject.GetComponent<TMP_Text>().text = "HIGH-SCORE: " + GameManager.instance.GetBestScore().ToString();
                DisplayAchievement(0);
                break; }
            case 2: { // Loading Screen
                menuUI.SetActive(false);
                Instantiate(Resources.Load<GameObject>("LoadingScreen"));
                break; }
            case 3: { // Settings
                menuUI.SetActive(false);
                settingsUI.SetActive(true);
                creditsUI.SetActive(false);
                break; }
            case 4: { // Gallery
                menuUI.SetActive(false);
                galleryUI.SetActive(true);
                GameManager.galleryManager.UpdateGalleryUI();
                GameManager.galleryManager.AlternateGalleryMenus(true);
                break;
            }  
            case 5: { // Credits
                creditsUI.SetActive(true);
                settingsUI.SetActive(false);
                break;
            }     
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

    // Function to ask the user to confirm their choice on an important UI choice.
    public void MenuConfirmationMessage(int cID) { 
        confirmationUIID = cID;
        TMP_Text message = confirmUI.transform.GetChild(1).GetComponent<TMP_Text>();

        // Set confirmation message based on scenario:
        switch (confirmationUIID) {
            case 0: { // Quit Application Confirmation.
                    message.text = "exit application?"; 
                    break; }
            case 1: { // Reset Data Confirmation.
                    message.text = "delete all your saved data?\nthis cannot be undone.";
                    break; }
        } confirmUI.SetActive(true);
    }

    // Funciton to carry out the appropiate UI response based on the confirmation response.
    public void MenuConfirmationResponse(bool response) {
        confirmUI.SetActive(false);
        if(response) { switch (confirmationUIID) { // Player chose "yes", so execute corresponding action.
            case 0: { // Quit Application.
                QuitApplication(); break; }
            case 1: { // Reset High Score.
                EraseData(); break; }
        }}
    }

    // Function to delete player's progress on request.
    public void EraseData() {
        GameManager.audioManager.PlayParcelSound(false);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        GameManager.instance.SetBestScore(0);
        GameManager.obstacleData.ResetEncounters();
        GameManager.achievementManager.UpdateData(true);
    }

    // Function to close the game application. 
    public void QuitApplication() { Application.Quit(); }    
}