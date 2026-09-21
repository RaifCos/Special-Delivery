using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using TMPro;

public class UnlockCutsceneManager : MonoBehaviour {
    private static readonly WaitForSeconds pauseTime = new(1.5f);
    private static readonly WaitForSeconds musicTime = new(2f);

    [Header ("UI Elements")]
    [SerializeField] private GameObject group;
    [SerializeField] private GameObject nextButton;

    [Header ("Unlock Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header ("Unlock Visuals")]
    [SerializeField] private GameObject iconImage;
    [SerializeField] private GameObject modelImage;
    [SerializeField] private Transform modelParent;

    [Header ("Visual/Audio Details")]
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
        string title = "";
        string desc = "";

        bool useIcon = cutsceneType == "achievement";
        iconImage.SetActive(useIcon);
        modelImage.SetActive(!useIcon);

        Level_SO level;

        switch (cutsceneType) {
            case "achievement":
                Achievement_SO achievement = GameManager.dataManager.GetAchievement(key);
                title = "you've completed the achievement \"" + achievement.externalName  + "\"!"; 
                desc = achievement.description;
                iconSprite.sprite = achievement.sprite;
                break;
            case "boss":
                level = GameManager.dataManager.GetLevel(key);
                title = "you've unlocked the boss battle for " + level.externalName + "!"; 
                desc = "deliver " + level.bossUnlockScore + " parcels in " + level.externalName;
                SetModel(modelParent.Find("boss").gameObject);
                break;
            case "level":
                level = GameManager.dataManager.GetLevel(key);
                title = "you can now deliver parcels in " + level.externalName + "!"; 
                desc = "Complete " + GameManager.dataManager.LevelUnlockList(key);
                SetModel(modelParent.Find(key).gameObject);
                break;
            case "garage":
                title = "you've unlocked van upgrades and the garage!"; 
                desc = "deliver 25 parcels";
                SetModel(modelParent.Find("player").gameObject);
                break;
            case "unlock":
                string[] res = UnlockText(key);
                title =  "you've unlocked the " + res[0];
                desc = "collect " + res[1];
                break;
        }
        
        titleText.text = title;
        descriptionText.text = "(" + desc + ")";
    }

    private string[] UnlockText(string key) {
        string[] res = new string[2];

        switch (key) {
            case "obstacleGallery":
                res[0] = "obstacle gallery!";
                res[1] = "1 stamp.";
                break;
            case "propGallery":
                res[0] = "prop gallery!";
                res[1] = "3 stamps.";
                break;
        } return res;
    }

    private void SetModel(GameObject newModel) {
        if (currentModel != null) currentModel.SetActive(false);
        currentModel = newModel;
        currentModel.SetActive(true);
    }

    public void NextClicked() {
        GameManager.audioManager.PlayParcelSound(true);
        StartCoroutine(ResetClip());
    }

    private IEnumerator ResetClip() {
        yield return Cutscene(false);
        PlayNext();
    }

    private IEnumerator Cutscene(bool opening) {
        if (opening) StartCoroutine(GameManager.audioManager.UnlockFanfare(fanfare)); 

        int start = opening? 255 : 0;
        int target = opening? 0 : 255;
        int rate = opening? -5 : 5;

        if (opening) DisplayUnlock();

        int alpha = start;
        while ((rate < 0 && alpha > target) || (rate > 0 && alpha < target)) {
            alpha += rate;
            panel.color = new Color(0, 0, 0, alpha / 255f);
            yield return null;
        } 
        
        group.SetActive(opening);

        if (opening) { yield return musicTime; }
        else { yield return pauseTime; }

        nextButton.SetActive(opening);
        eventSystem.SetSelectedGameObject(nextButton);
    }
}
