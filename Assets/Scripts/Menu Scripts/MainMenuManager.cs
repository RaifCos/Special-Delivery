using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.VisualScripting;

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
    [SerializeField] private Button playButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button bossButton;
    [SerializeField] private Image backdrop;
    [SerializeField] private TMP_Text levelName;
    [SerializeField] private GameObject levelSelection;
    [SerializeField] private Sprite lockedSprite;
    private RectTransform levelSelectionRect;
    private Navigation playNav, tutorialNav;

    [Header ("UI Navigation")]
    [SerializeField] private GameObject navStartSelected;
    [SerializeField] private GameObject achievementsStartSelected;
    [SerializeField] private GameObject levelStartSelected;
    [SerializeField] private GameObject shopStartSelected;
    [SerializeField] private GameObject settingsStartSelected;
    [SerializeField] private GameObject confirmStartSelected;
    private EventSystem eventSystem;
    private Level_SO selectedLevel;
    private int selectedLevelNumber = 0;
    private float targetScrollerPos;

    [Header ("Music")]
    [SerializeField] private AudioClip musicStart;
    [SerializeField] private AudioClip musicLoop;
    private int confirmationUIID;

    void OnEnable() { eventSystem = EventSystem.current; }

    void Awake() { GameManager.mainMenuManager = this; }

    private void Start() {
        levelSelectionRect = levelSelection.GetComponent<RectTransform>();
        playNav = playButton.navigation;
        tutorialNav = tutorialButton.navigation;
        GameManager.audioManager.Initalize(musicStart, musicLoop); 
        ToggleBossLock(GameManager.dataManager.GetLevelProgress("city") > 2);
        ToggleShopLock(GameManager.dataManager.IsShopUnlocked());
        AlternateMainMenus(0);
        StartCoroutine(SelectInitialButton());
    }

    private void LateUpdate() {
        float pos = levelSelectionRect.anchoredPosition.x;
        if (Mathf.Approximately(pos, targetScrollerPos)) { return; }

        float newX = Mathf.MoveTowards(pos, targetScrollerPos, 1000f * Time.deltaTime);
        levelSelectionRect.anchoredPosition = new(newX, levelSelectionRect.anchoredPosition.y);
    }

    public void StartGame(int difficulty) {
        GameManager.instance.SetDifficulty(difficulty);
        AlternateMainMenus(6);
        StartCoroutine(GameManager.instance.LoadAsyncScene(selectedLevel.internalName));
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
                UpdateLevelSelectMenu();
                menuUI.SetActive(false);
                levelSelectUI.SetActive(true);
                levelSelectionRect.anchoredPosition = new(-335f, levelSelectionRect.anchoredPosition.y);
                eventSystem.SetSelectedGameObject(levelStartSelected);
                DisplayLevel("city");
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

    public void ToggelPlayLock(bool isUnlocked) {
        playButton.interactable = isUnlocked;
        tutorialButton.interactable = isUnlocked;
        if(isUnlocked) {
            playButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "NORMAL SHIFT";
            playButton.GetComponent<MenuText>().message = "RACE AGAINST THE CLOCK TO DELIVER AS MANY PARCELS AS YOU CAN";
            tutorialButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "PRACTICE MODE";
            tutorialButton.GetComponent<MenuText>().message = "LEARN THE BASICS- NO OBSTACLES, NO TIMER, NO WORRIES";
        } else {
            playButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "???";
            playButton.GetComponent<MenuText>().message = "LOCKED FOR NOW...";
            tutorialButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "???";
            tutorialButton.GetComponent<MenuText>().message = "LOCKED FOR NOW...";
        }
    }

    public void ToggleBossLock(bool isUnlocked) {
        bossButton.interactable = isUnlocked;
        if(isUnlocked) {
            bossButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "BOSS BATTLE";
            bossButton.GetComponent<MenuText>().message = "FACE OFF AGAINST A RIVAL DELIVERY VAN. FIRST TO 5 DELIVERIES WINS!";
        } else {
            bossButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "???";
            bossButton.GetComponent<MenuText>().message = "COMPLETE 40 DELIVERIES TO UNLOCK";
        }
    }

    // Lock UI with special "Coming Soon" setup for future Levels.
    public void ComingSoonSetup() {
        playButton.interactable = false;
        tutorialButton.interactable = false;
        bossButton.interactable = false;
        playButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "???";
        playButton.GetComponent<MenuText>().message = "COMING SOON...";
        tutorialButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "???";
        tutorialButton.GetComponent<MenuText>().message = "COMING SOON...";
        bossButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "???";
        bossButton.GetComponent<MenuText>().message = "COMING SOON...";

        levelName.text = "Coming Soon";
    }

    public void DisplayLevel(string lvl) {
        // If Level Name contains (%d) then it is a Coming Soon placeholder.
        Match match = Regex.Match(lvl, @"\((\d+)\)");
        if (match.Success) {
            ComingSoonSetup();
            int res = int.Parse(match.Groups[1].Value);
            SetLevelSelectNav(res);
            targetScrollerPos = -335 + res * -695;
            return;
        }

        // Otherwise Display as Normal.
        selectedLevel = GameManager.dataManager.GetLevel(lvl);
        selectedLevelNumber = selectedLevel.levelNumber;
        int selectedLevelProgress = GameManager.dataManager.GetLevelProgress(lvl);

        targetScrollerPos = -335 + selectedLevelNumber * -695;
        ToggelPlayLock(selectedLevelProgress > 0);
        ToggleBossLock(selectedLevelProgress > 1);
        SetLevelSelectNav(selectedLevelNumber);

        levelName.text = selectedLevelProgress > 0? selectedLevel.externalName: "???";
    }

    private void SetLevelSelectNav(int input) {
        Button currentIcon = levelSelection.transform.GetChild(input).GetComponent<Button>();

        playNav.selectOnUp = currentIcon;
        playButton.navigation = playNav;

        tutorialNav.selectOnUp = currentIcon;
        tutorialButton.navigation = tutorialNav;
    }

    public void UpdateLevelSelectMenu() {
        foreach(Level_SO lvl in GameManager.dataManager.GetLevels()) {
            UpdateLevelUI(lvl);
        }
    }

    // Function to update the UI in the Level Menu based on the Level's state.
    private void UpdateLevelUI(Level_SO lvl) {
        string key = lvl.internalName;
        GameObject obj = levelSelection.transform.Find(key).gameObject;
        Image img = obj.GetComponent<Image>();
        if (GameManager.dataManager.GetLevelProgress(key) != 0) { 
            img.sprite = lvl.sprite;
            obj.GetComponent<MenuText>().message = lvl.description;
        } else { 
            img.sprite = lockedSprite;
            obj.GetComponent<MenuText>().message = "LOCKED FOR NOW...";
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