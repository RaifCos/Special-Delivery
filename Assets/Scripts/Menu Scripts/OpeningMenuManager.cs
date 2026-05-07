using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Script to handle main game functionality.
public class OpeningMenuManager : MonoBehaviour {

    [Header ("UI Canvases")]
    [SerializeField] private GameObject openingUI;
    [SerializeField] private GameObject creditsUI;
    [SerializeField] private GameObject fileUI;
    [SerializeField] private GameObject confirmUI;

    [Header ("UI Navigation")]
    [SerializeField] private GameObject openingStartSelected;
    [SerializeField] private GameObject creditsStartSelected;
    [SerializeField] private GameObject fileStartSelected;
    [SerializeField] private GameObject confirmStartSelected;
    private EventSystem eventSystem;

    [Header ("Music")]
    [SerializeField] private AudioClip musicStart;
    [SerializeField] private AudioClip musicLoop;

    private GameObject[] saveFileUI;
    private ProgressData[] saveFileProgress = new ProgressData[3]; 
    private Color32 completeColor = new(255, 227, 0, 255);

    void OnEnable() { eventSystem = EventSystem.current; }

    void Awake() { GameManager.openingMenuManager = this; }

    public void Start() {
        AlternateOpeningMenus(0);
        saveFileProgress = GameManager.dataManager.LoadSaveFiles();
        UpdateSaveFileUI();
        GameManager.audioManager.Initalize(musicStart, musicLoop); 
    }

    public void OpenGame(int saveFile) {
        GameManager.instance.SetSaveFile(saveFile);
        AlternateOpeningMenus(2);
        StartCoroutine(GameManager.instance.LoadAsyncScene("MainMenu"));
    }

    // Function to alterante between the UI Menus.
    public void AlternateOpeningMenus(int menu) {
        switch (menu) {
            case 0: { // Opening Menu
                openingUI.SetActive(true);
                creditsUI.SetActive(false);
                fileUI.SetActive(false);
                eventSystem.SetSelectedGameObject(openingStartSelected);
                break; }
            case 1: { // Credits
                openingUI.SetActive(false);
                creditsUI.SetActive(true);
                eventSystem.SetSelectedGameObject(creditsStartSelected);
                break; }
            case 2: { // Loading Screen
                fileUI.SetActive(false);
                Instantiate(Resources.Load<GameObject>("LoadingScreen"));
                break; }
            case 3: { // File Select
                openingUI.SetActive(false);
                fileUI.SetActive(true);
                eventSystem.SetSelectedGameObject(fileStartSelected);
                break; }
        }  
    }

    // Function to ask the user to confirm their choice on an important UI choice.
    public void MenuConfirmationMessage() { 
        TMP_Text message = confirmUI.transform.GetChild(3).GetComponent<TMP_Text>();
        message.text = "exit the game?"; 
        confirmUI.SetActive(true);
        eventSystem.SetSelectedGameObject(confirmStartSelected);
    }

    // Funciton to carry out the appropiate UI response based on the confirmation response.
    public void MenuConfirmationResponse(bool response) {
        confirmUI.SetActive(false);
        if(response) { QuitApplication(); }
        else { eventSystem.SetSelectedGameObject(openingStartSelected); }
    }

    // Function to close the game application. 
    public void QuitApplication() { Application.Quit(); }

    private void UpdateSaveFileUI() {
        for (int i = 0; i < saveFileProgress.Length; i++) {
            Transform panel = saveFileUI[i].transform;
            if (!saveFileProgress[i].isEmpty) {
                // Activate Progress UI Elements.
                ActivateSaveFileUIElement(panel.GetChild(0).gameObject, "", saveFileProgress[i].totalProgress);
                ActivateSaveFileUIElement(panel.GetChild(2).gameObject, "Gallery\t\t", saveFileProgress[i].galleryProgress);
                ActivateSaveFileUIElement(panel.GetChild(3).gameObject, "Achievements\t", saveFileProgress[i].achievementProgress);
                
                // Show Upgrades only if the Garage is Unlocked. 
                string upgradeString;
                if (saveFileProgress[i].shopUnlocked) { upgradeString = "Upgrades\t\t"; }
                else { upgradeString = "???\t\t\t"; }
                ActivateSaveFileUIElement(panel.GetChild(1).gameObject, upgradeString, saveFileProgress[i].upgradeProgress);
            
                // Disable "No Save Data" Text.
                panel.GetChild(4).gameObject.SetActive(false);
                panel.GetChild(5).gameObject.transform.GetChild(0).GetComponent<TMP_Text>().text = "Play";
            } else {
                panel.GetChild(0).gameObject.SetActive(false);
                panel.GetChild(1).gameObject.SetActive(false);
                panel.GetChild(2).gameObject.SetActive(false);
                panel.GetChild(3).gameObject.SetActive(false);
                panel.GetChild(4).gameObject.SetActive(true);
                panel.GetChild(5).gameObject.transform.GetChild(0).GetComponent<TMP_Text>().text = "New Game";
            }
        } 
    }

    private void ActivateSaveFileUIElement(GameObject element, string name, int score) {
        element.SetActive(true);
        TMP_Text elementText = element.GetComponent<TMP_Text>();
        elementText.text = name + score + "%";
        if(score == 100) { elementText.color =  completeColor; }
    }
}