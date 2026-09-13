using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using TMPro;

public class UnlockCutsceneManager : MonoBehaviour {
    private static readonly WaitForSeconds pauseTime = new(1f);
    private static readonly WaitForSeconds musicTime = new(3.5f);
    [SerializeField] private GameObject iconImage;
    [SerializeField] private GameObject modelImage;
    [SerializeField] private Transform modelParent;
    [SerializeField] private GameObject group;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private TMP_Text unlockText;
    [SerializeField] private Image panel;
    [SerializeField] private AudioClip fanfare;
    private string currentCutscene;
    private Image iconSprite;
    private EventSystem eventSystem;
    private GameObject currentModel;

    void Awake() {
        GameManager.unlockCutsceneManager = this;
        eventSystem = EventSystem.current; 
    }

    private void Start() {
        iconSprite = iconImage.GetComponent<Image>();
        PlayNext();
    }

    private void PlayNext() {
        currentCutscene = GameManager.dataManager.DequeueCutscene();
        if (currentCutscene == null) { StartCoroutine(GameManager.instance.LoadAsyncScene("MainMenu")); }
        else StartCoroutine(Cutscene(true));
    }

    public void DisplayUnlock() {
        Match match = Regex.Match(currentCutscene, @"^(.+)-(.+)$");
        string cutsceneType = match.Groups[1].Value;
        string key = match.Groups[2].Value;
        string message = "";

        bool useIcon = cutsceneType == "achievement";
        iconImage.SetActive(useIcon);
        modelImage.SetActive(!useIcon);

        Level_SO level;

        switch (cutsceneType) {
            case "achievement":
                Achievement_SO achievement = GameManager.dataManager.GetAchievement(key);
                message = "you've completed the achievement \"" + achievement.externalName  + "\"!"; 
                iconSprite.sprite = achievement.sprite;
                break;
            case "boss":
                level = GameManager.dataManager.GetLevel(key);
                message = "you've unlocked the boss battle for " + level.externalName + "!"; 
                SetModel(modelParent.Find("boss").gameObject);
                break;
            case "level":
                level = GameManager.dataManager.GetLevel(key);
                message = "you can now deliver parcels in " + level.externalName + "!"; 
                SetModel(modelParent.Find(key).gameObject);
                break;
            case "garage":
                message = "you've unlocked van upgrades and the garage!"; 
                SetModel(modelParent.Find("player").gameObject);
                break;
        } unlockText.text = message;
    }

    private void SetModel(GameObject newModel) {
        if (currentModel != null) currentModel.SetActive(false);
        currentModel = newModel;
        currentModel.SetActive(true);
    }

    public void NextClicked() => StartCoroutine(ResetClip());

    private IEnumerator ResetClip() {
        GameManager.audioManager.PlayParcelSound(true);
        yield return Cutscene(false);
        PlayNext();
    }

    private IEnumerator Cutscene(bool opening) {
        int start = opening? 255 : 0;
        int target = opening? 0 : 255;
        int rate = opening? -5 : 5;

        if (opening) { DisplayUnlock(); }

        int alpha = start;
        while ((rate < 0 && alpha > target) || (rate > 0 && alpha < target)) {
            alpha += rate;
            panel.color = new Color(0, 0, 0, alpha / 255f);
            yield return null;
        } group.SetActive(opening);

        if (opening) {
            StartCoroutine(GameManager.audioManager.UnlockFanfare(fanfare)); 
            yield return musicTime; 
        } else { yield return pauseTime; }

        nextButton.SetActive(opening);
        eventSystem.SetSelectedGameObject(nextButton);
    }
}
