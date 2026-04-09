using TMPro;
using UnityEngine;

// Script to handle main game functionality.
public class OpeningMenuManager : MonoBehaviour {
    public GameObject openingUI, creditsUI, confirmUI;
    private static int confirmationUIID;

    void Awake() { GameManager.openingMenuManager = this; }

    public void Start() {
        AlternateOpeningMenus(0);
        StartCoroutine(GameManager.audioManager.StartGameMusic());
    }

    public void OpenGame() {
        AlternateOpeningMenus(2);
        StartCoroutine(GameManager.instance.LoadAsyncScene("MainMenu"));
    }

    // Function to alterante between the UI Menus.
    public void AlternateOpeningMenus(int menu) {
        switch (menu) {
            case 0: { // Opening Menu
                openingUI.SetActive(true);
                creditsUI.SetActive(false);
                break; }
            case 1: { // Credits
                openingUI.SetActive(false);
                creditsUI.SetActive(true);
                break; }
            case 2: { // Loading Screen
                openingUI.SetActive(false);
                Instantiate(Resources.Load<GameObject>("LoadingScreen"));
                break; }
        }  
    }

    // Function to ask the user to confirm their choice on an important UI choice.
    public void MenuConfirmationMessage() { 
        TMP_Text message = confirmUI.transform.GetChild(3).GetComponent<TMP_Text>();
        message.text = "exit the game?"; 
        confirmUI.SetActive(true);
    }

    // Funciton to carry out the appropiate UI response based on the confirmation response.
    public void MenuConfirmationResponse(bool response) {
        confirmUI.SetActive(false);
        if(response) { 
            switch (confirmationUIID) { // Player chose "yes", so execute corresponding action.
            case 0: { // Quit Application.
                QuitApplication(); break; }
        }}
    }

    // Function to close the game application. 
    public void QuitApplication() { Application.Quit(); }  

}