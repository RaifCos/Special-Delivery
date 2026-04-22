using UnityEngine;
using UnityEngine.EventSystems;

// Script to handle main game functionality.
public class SettingsManager : MonoBehaviour {
    public GameObject settingsUI, lowerButton, higherButton;
    private EventSystem eventSystem;

    void Awake() { 
        GameManager.settingsManager = this;
        eventSystem = EventSystem.current;    
    }

    public void SetShadows(bool beingSetLower) {
        lowerButton.SetActive(!beingSetLower);
        higherButton.SetActive(beingSetLower);
        if (beingSetLower) { eventSystem.SetSelectedGameObject(higherButton); }
        else { eventSystem.SetSelectedGameObject(lowerButton); }
        GameManager.instance.ToggleShadows(beingSetLower);
    }

    // Function to delete player's progress on request.
    public void EraseData() {
        GameManager.mainMenuManager.AlternateMainMenus(6);
        GameManager.dataManager.ResetData();
        GameManager.dataManager.SetShopProgress(false);
        GameManager.audioManager.PlayParcelSound(false);
        StartCoroutine(GameManager.instance.LoadAsyncScene("OpeningMenu"));
    }
}