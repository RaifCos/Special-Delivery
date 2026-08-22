using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Script to handle achievement tracking and the achievement menu UI.
public class AchievementMenuManager : MonoBehaviour {

    [Header("Achievement Menu Variables")]
    public GameObject buttonIcons;
    public GameObject achievementDisplay;
    public Sprite lockedSprite;

    // Variables used for tracking achievements.
    private int lifetimeDeliveries, playerCrashes;

    void Awake() { GameManager.achievementMenuManager = this; }

    void Start() {
        lifetimeDeliveries = GameManager.dataManager.GetLifetimeDeliveries();
        playerCrashes = GameManager.dataManager.GetPlayerCrashes();
    }

    public void UpdateAchievementMenu() {
        foreach(Achievement_SO ach in GameManager.dataManager.GetAchievements()) {
            UpdateAchievementUI(ach.internalName);
        }
    }

    // Function to update the UI in the Achievement Menu based on the Achievement's state.
    private void UpdateAchievementUI(string key) {
        Image img = buttonIcons.transform.Find(key).GetComponent<Image>();
        if (GameManager.dataManager.AchievementState(key) == 2) { 
            img.sprite = GameManager.dataManager.GetAchievement(key).sprite;
        } else { img.sprite = lockedSprite; }
    }
    
    public void DisplayAchievement(string key) {
        Image img = buttonIcons.transform.Find(key).GetComponent<Image>();
        achievementDisplay.transform.GetChild(0).GetComponent<Image>().sprite = img.sprite;
        Achievement_SO ach = GameManager.dataManager.GetAchievement(key);
        // Achievement is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            achievementDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = "???";
        } else { // Achievement is unlocked, so show achievement information.
            achievementDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = ach.externalName;
        } 
        
        string res;
        if (GameManager.dataManager.AchievementState(key) > 0) {
            res = ach.description;
            // For certain achievements, display the associated tracking variable for clarity.
            switch (key) {
            case "lifetime250": { // Lifetime Deliveries
                    res += " [" + lifetimeDeliveries + "]";
                    break; }
            case "crash1000":
            case "crash10000": { // Player Crashes
                    res += " [" +  playerCrashes + "]";
                    break; }}
        } else { res = "you'll need to complete another achievement first."; }

        achievementDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = res;
    }  

}