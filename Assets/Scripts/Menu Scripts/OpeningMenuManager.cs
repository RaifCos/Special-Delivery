using TMPro;
using UnityEngine;

// Script to handle main game functionality.
public class OpeningMenuManager : MonoBehaviour {
    public GameObject openingUI, settingsUI, creditsUI, confirmUI;
    private static int confirmationUIID;

    void Awake() { GameManager.openingMenuManager = this; }

    public void Start() {
        AlternateOpeningMenus(0);
        StartCoroutine(GameManager.audioManager.StartGameMusic());
    }

    public void OpenGame() {
        AlternateOpeningMenus(3);
        StartCoroutine(GameManager.instance.LoadAsyncScene("MainMenu"));
    }

    // Function to alterante between the UI Menus.
    public void AlternateOpeningMenus(int menu) {
        switch (menu) {
            case 0: { // Opening Menu
                openingUI.SetActive(true);
                settingsUI.SetActive(false);
                break; }
            case 1: { // Settings
                openingUI.SetActive(false);
                settingsUI.SetActive(true);
                creditsUI.SetActive(false);
                break; }
            case 2: { // Credits
                settingsUI.SetActive(false);
                creditsUI.SetActive(true);
                break; }
            case 3: { // Loading Screen
                openingUI.SetActive(false);
                Instantiate(Resources.Load<GameObject>("LoadingScreen"));
                break; }
        }  
    }

    // Function to ask the user to confirm their choice on an important UI choice.
    public void MenuConfirmationMessage(int cID) { 
        confirmationUIID = cID;
        TMP_Text message = confirmUI.transform.GetChild(3).GetComponent<TMP_Text>();

        // Set confirmation message based on scenario:
        switch (confirmationUIID) {
            case 0: { // Quit Application Confirmation.
                    message.text = "exit the game?"; 
                    break; }
            case 1: { // Reset Data Confirmation.
                    message.text = "delete all your saved data?\nthis cannot be undone.";
                    break; }
        } confirmUI.SetActive(true);
    }

    // Funciton to carry out the appropiate UI response based on the confirmation response.
    public void MenuConfirmationResponse(bool response) {
        confirmUI.SetActive(false);
        if(response) { 
            switch (confirmationUIID) { // Player chose "yes", so execute corresponding action.
            case 0: { // Quit Application.
                QuitApplication(); break; }
            case 1: { // Reset All Data.
                EraseData(); break; }
        }}
    }

    // Function to delete player's progress on request.
    public void EraseData() {
        GameManager.audioManager.PlayParcelSound(false);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        GameManager.instance.SetBestScore(0);
        GameManager.instance.SetShopProgress(false);
    }

    // Function to close the game application. 
    public void QuitApplication() { Application.Quit(); }  

}