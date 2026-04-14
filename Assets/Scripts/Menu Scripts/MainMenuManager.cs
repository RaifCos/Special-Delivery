using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Script to handle main game functionality.
public class MainMenuManager : MonoBehaviour {
    public GameObject menuUI, levelSelectUI, achievementUI, galleryUI, garageUI, confirmUI, settingsUI, navDescription;
    public Button shopButton;
    public Image backdrop;
    private int confirmationUIID;
    void Awake() { GameManager.mainMenuManager = this; }

    public void Start() {
        ToggleShopLock(GameManager.dataManager.IsShopUnlocked());
        StartCoroutine(GameManager.audioManager.StartGameMusic());
        AlternateMainMenus(0);
    }

    public void StartGame(int difficulty) {
        GameManager.instance.SetDifficulty(difficulty);
        AlternateMainMenus(6);
        StartCoroutine(GameManager.instance.LoadAsyncScene("City"));
    }

    // Function to alterante between the UI Menus.
    public void AlternateMainMenus(int menu) {
        switch (menu) {
            case 0: { // Main Menu
                backdrop.color = new Color32(62, 123, 230, 255);
                navDescription.GetComponent<TMP_Text>().text = "";
                menuUI.SetActive(true);
                garageUI.SetActive(false);
                levelSelectUI.SetActive(false);
                galleryUI.SetActive(false);
                achievementUI.SetActive(false);
                settingsUI.SetActive(false);
                break; }
            case 1: { // Gallery 
                menuUI.SetActive(false);
                galleryUI.SetActive(true);
                backdrop.color = new Color32(93, 105, 208, 255);
                GameManager.galleryManager.UpdateGalleryUI();
                GameManager.galleryManager.AlternateGalleryMenus(true);
                break; }
            case 2: { // Achievements
                GameManager.achievementMenuManager.UpdateAchievementMenu();
                backdrop.color = new Color32(39, 191, 200, 255);
                menuUI.SetActive(false);
                achievementUI.SetActive(true);
                achievementUI.transform.GetChild(3).gameObject.GetComponent<TMP_Text>().text = "HIGH-SCORE: " + GameManager.dataManager.GetBestScore().ToString();
                GameManager.achievementMenuManager.DisplayAchievement("score10");
                break; }
            case 4: { // Level Select
                menuUI.SetActive(false);
                levelSelectUI.SetActive(true);
                break; }    
            case 5: { // Shop 
                backdrop.color = new Color32(62, 204, 230, 255);
                GameManager.garageMenuManager.UpdateMenu();
                GameManager.garageMenuManager.DisplayUpgrade("booster");
                menuUI.SetActive(false);
                garageUI.SetActive(true);
                break; }    
            case 6: { // Loading Screen
                menuUI.SetActive(false);
                confirmUI.SetActive(false);
                Instantiate(Resources.Load<GameObject>("LoadingScreen"));
                break; }
            case 7: { // Settings
                backdrop.color = new Color32(20, 58, 123, 255);
                menuUI.SetActive(false);
                settingsUI.SetActive(true);
                break; }
        }  
    }

    public void ToggleShopLock(bool isUnlocked) {
        shopButton.interactable = isUnlocked;
        if(isUnlocked) {
            shopButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "GARAGE";
            shopButton.GetComponent<ButtonHover>().message = "BUY NIFTY UPGRADES FOR YOUR DELIVERY VAN";
        } else {
            shopButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "???";
            shopButton.GetComponent<ButtonHover>().message = "COMPLETE 25 DELIVERIES TO UNLOCK.";
        }
    }

    // Function to ask the user to confirm their choice on an important UI choice.
    public void MenuConfirmationMessage(int cID) {
        confirmationUIID = cID;
        TMP_Text message = confirmUI.transform.GetChild(2).GetComponent<TMP_Text>();
        switch(confirmationUIID) {
            case 0: {
                backdrop.color = new Color32(20, 58, 123, 255);
                message.text = "return to the menu?";
                menuUI.SetActive(false);
                break; }
            case 1: {
                message.text = "delete all your save data? this cannot be undone!";
                settingsUI.SetActive(false);
                break; }
        }
        confirmUI.SetActive(true);
    }

    // Funciton to carry out the appropiate UI response based on the confirmation response.
    public void MenuConfirmationResponse(bool response) {
        confirmUI.SetActive(false);
        switch(confirmationUIID) {
            case 0: {
                if(response) {
                    AlternateMainMenus(6);
                    StartCoroutine(GameManager.instance.LoadAsyncScene("OpeningMenu"));
                } else { AlternateMainMenus(0); }
                break; }
            case 1: {
                if(response) {
                    GameManager.settingsManager.EraseData();
                } else { AlternateMainMenus(7); }
                break; }
        }
    }
}