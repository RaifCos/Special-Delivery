using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Script to handle main game functionality.
public class MainMenuManager : MonoBehaviour {
    public GameObject menuUI, levelSelectUI, achievementUI, galleryUI, shopUI, confirmUI, navDescription;
    public Button shopButton;
    public Image backdrop;

    void Awake() { GameManager.mainMenuManager = this; }

    public void Start() {
        GameManager.dataManager.LoadAchievementData();
        ToggleShopLock(GameManager.instance.GetShopProgress());
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
                shopUI.SetActive(false);
                levelSelectUI.SetActive(false);
                galleryUI.SetActive(false);
                achievementUI.SetActive(false);
                break; }
            case 1: { // Achievements 
                GameManager.achievementMenuManager.UpdateAchievementMenu();
                backdrop.color = new Color32(39, 191, 200, 255);
                menuUI.SetActive(false);
                achievementUI.SetActive(true);
                achievementUI.transform.GetChild(3).gameObject.GetComponent<TMP_Text>().text = "HIGH-SCORE: " + GameManager.instance.GetBestScore().ToString();
                GameManager.achievementMenuManager.DisplayAchievement("score10");
                break; }
            case 2: { // Gallery
                menuUI.SetActive(false);
                galleryUI.SetActive(true);
                backdrop.color = new Color32(93, 105, 208, 255);
                GameManager.galleryManager.UpdateGalleryUI();
                GameManager.galleryManager.AlternateGalleryMenus(true);
                break; }
            case 4: { // Level Select
                menuUI.SetActive(false);
                levelSelectUI.SetActive(true);
                    break;
                }    
            case 5: { // Shop 
                    backdrop.color = new Color32(62, 204, 230, 255);
                    shopUI.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text = string.Format("{0:#,##0.##}", GameManager.instance.GetMoney());
                    menuUI.SetActive(false);
                    shopUI.SetActive(true);
                    break;
                }    
            case 6: { // Loading Screen
                menuUI.SetActive(false);
                confirmUI.SetActive(false);
                Instantiate(Resources.Load<GameObject>("LoadingScreen"));
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
    public void MenuConfirmationMessage() {
        backdrop.color = new Color32(20, 58, 123, 255);
        TMP_Text message = confirmUI.transform.GetChild(2).GetComponent<TMP_Text>();
        message.text = "return to the menu?";
        menuUI.SetActive(false);
        confirmUI.SetActive(true);
    }

    // Funciton to carry out the appropiate UI response based on the confirmation response.
    public void MenuConfirmationResponse(bool response) {
        confirmUI.SetActive(false);
        if (response) { 
            AlternateMainMenus(6);
            StartCoroutine(GameManager.instance.LoadAsyncScene("OpeningMenu"));
        } else { AlternateMainMenus(0); }
    }
}