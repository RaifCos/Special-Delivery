using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Script to handle achievement tracking and the achievement menu UI.
public class AchievementMenuManager : MonoBehaviour {

    [Header("Achievement Menu Variables")]
    public GameObject buttonIcons;
    public GameObject achievementDisplay;
    public Sprite lockedSprite;
    public List<Sprite> achievementSprite;

    private List<Achievement_SO> achievements;

    // Variables used for tracking achievements.
    public int lifetimeDeliveries, playerCrashes;

    void Awake() {
        GameManager.achievementMenuManager = this;
    }

    void Start() {
        achievements = GameManager.achievementData.GetAchievements();
    }

    public void UpdateAchievementMenu() {
        foreach(Achievement_SO ach in achievements) {
            UpdateAchievementUI(ach.internalName);
        }
    }

    // Function to update the UI in the Achievement Menu based on the Achievement's state.
    private void UpdateAchievementUI(string key) {
        Image img = buttonIcons.transform.Find(key).GetComponent<Image>();
        if (GameManager.achievementData.IsAchieved(key)) { 
            img.sprite = achievementSprite.Find(sprite => sprite.name == key);
        } else { img.sprite = lockedSprite; }
    }
    
    public void DisplayAchievement(string key) {
        Debug.Log(key);
        Image img = buttonIcons.transform.Find(key).GetComponent<Image>();
        achievementDisplay.transform.GetChild(0).GetComponent<Image>().sprite = img.sprite;
        Achievement_SO ach = achievements.Find(ach => ach.internalName == key);
        // Achievement is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            achievementDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = "???";
        } else { // Achievement is unlocked, so show achievement information.
            achievementDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = ach.externalName;
        } 
        
        // For certain achievements, display the associated tracking variable for clarity.
        string res = ach.description;
        switch (key) {
            case "lifetime250": { // Lifetime Deliveries
                    res += " [" + GameManager.achievementData.GetLifetimeScore() + "]";
                    break; }
            case "crash100":
            case "crash250": { // Player Crashes
                    res += " [" +  GameManager.achievementData.GetPlayerCrashes() + "]";
                    break; }}
        achievementDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = res;
    }  

}