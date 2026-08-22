using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour {
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private TMP_Dropdown controllerScheme;
    [SerializeField] private Toggle shadowToggle;

    private bool qualityShadows; 

    void Awake() => GameManager.settingsManager = this;  

    public void SetShadows() => qualityShadows = shadowToggle.isOn;

    public void SetControllerSchemeValue() => controllerScheme.value = GameManager.instance.GetControllerScheme();

    // Function to delete player's progress on request.
    public void EraseData() {
        GameManager.mainMenuManager.AlternateMainMenus(6);
        GameManager.dataManager.ResetData();
        GameManager.dataManager.SetShopProgress(false);
        GameManager.audioManager.PlayParcelSound(false);
        StartCoroutine(GameManager.instance.LoadAsyncScene("OpeningMenu"));
    }

    public void BackToMenu() {
        GameManager.audioManager.ConfirmMusicVolumeChange();
        GameManager.audioManager.ConfirmSoundEffectVolumeChange();
        GameManager.instance.SetControllerScheme(controllerScheme.value);
        GameManager.mainMenuManager.AlternateMainMenus(0);
        GameManager.instance.ToggleShadows(qualityShadows);
    }
}