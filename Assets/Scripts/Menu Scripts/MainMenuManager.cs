using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

// Script to handle main game functionality.
public class MainMenuManager : MonoBehaviour {
    [Header ("Menu Canvases")]
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject levelSelectUI;
    [SerializeField] private GameObject achievementUI;
    [SerializeField] private GameObject galleryUI;
    [SerializeField] private GameObject garageUI;
    [SerializeField] private GameObject confirmUI;
    [SerializeField] private GameObject settingsUI;

    [Header ("UI Elements")]
    [SerializeField] private GameObject navDescription;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button bossButton;
    [SerializeField] private Image backdrop;

    [Header ("UI Navigation")]
    [SerializeField] private GameObject navStartSelected;
    [SerializeField] private GameObject achievementsStartSelected;
    [SerializeField] private GameObject levelStartSelected;
    [SerializeField] private GameObject shopStartSelected;
    [SerializeField] private GameObject settingsStartSelected;
    [SerializeField] private GameObject confirmStartSelected;
    private EventSystem eventSystem;

    [Header ("Music")]
    [SerializeField] private AudioClip musicStart;
    [SerializeField] private AudioClip musicLoop;
    private int confirmationUIID;

    void OnEnable() { eventSystem = EventSystem.current; }

    void Awake() { GameManager.mainMenuManager = this; }

    public void Start() {
        GameManager.audioManager.Initalize(musicStart, musicLoop); 
        ToggleBossLock(GameManager.dataManager.GetLevelProgress("city") > 2);
        ToggleShopLock(GameManager.dataManager.IsShopUnlocked());
        AlternateMainMenus(0);
        StartCoroutine(SelectInitialButton());
    }

    public void StartGame(int difficulty) {
        GameManager.instance.SetDifficulty(difficulty);
        AlternateMainMenus(6);
        StartCoroutine(GameManager.instance.LoadAsyncScene("city"));
    }

    private IEnumerator SelectInitialButton() {
        yield return null;
        eventSystem.SetSelectedGameObject(navStartSelected);
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
                eventSystem.SetSelectedGameObject(navStartSelected);
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
                eventSystem.SetSelectedGameObject(achievementsStartSelected);
                break; }
            case 4: { // Level Select
                menuUI.SetActive(false);
                levelSelectUI.SetActive(true);
                eventSystem.SetSelectedGameObject(levelStartSelected);
                break; }    
            case 5: { // Shop 
                backdrop.color = new Color32(62, 204, 230, 255);
                GameManager.garageMenuManager.UpdateMenu(false);
                GameManager.garageMenuManager.DisplayUpgrade("booster");
                menuUI.SetActive(false);
                garageUI.SetActive(true);
                eventSystem.SetSelectedGameObject(shopStartSelected);
                break; }    
            case 6: { // Loading Screen
                eventSystem.SetSelectedGameObject(null);
                menuUI.SetActive(false);
                confirmUI.SetActive(false);
                Instantiate(Resources.Load<GameObject>("LoadingScreen"));
                break; }
            case 7: { // Settings
                backdrop.color = new Color32(20, 58, 123, 255);
                menuUI.SetActive(false);
                settingsUI.SetActive(true);
                eventSystem.SetSelectedGameObject(settingsStartSelected);
                break; }
        }  
    }

    public void ToggleShopLock(bool isUnlocked) {
        shopButton.interactable = isUnlocked;
        if(isUnlocked) {
            shopButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "GARAGE";
            shopButton.GetComponent<MenuText>().message = "BUY NIFTY UPGRADES FOR YOUR DELIVERY VAN";
        } else {
            shopButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "???";
            shopButton.GetComponent<MenuText>().message = "COMPLETE 25 DELIVERIES TO UNLOCK";
        }
    }

    public void ToggleBossLock(bool isUnlocked) {
        bossButton.interactable = isUnlocked;
        if(isUnlocked) {
            bossButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Boss Battle";
            bossButton.GetComponent<MenuText>().message = "FACE OFF AGAINST A RIVAL DELIVERY VAN. FIRST TO 5 DELIVERIES WINS!";
        } else {
            bossButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "???";
            bossButton.GetComponent<MenuText>().message = "COMPLETE 50 DELIVERIES TO UNLOCK";
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
        eventSystem.SetSelectedGameObject(confirmStartSelected);
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