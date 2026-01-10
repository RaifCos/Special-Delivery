using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Script to handle achievement tracking and the achievement menu UI.
public class AchievementMenuManager : MonoBehaviour {

    [Header("Achievement Menu Variables")]
    public GameObject buttonIcons;
    public GameObject achievementDisplay;
    public Sprite lockedSprite;
    public Sprite[] achievementSprite;

    private  Achievement[] achievements;

    // Variables used for tracking achievements.
    public int lifetimeDeliveries, playerCrashes;

    void Awake() {
        GameManager.achievementMenuManager = this;
    }

    void Start() {
        achievements = GameManager.achievementData.GetAchievements();
        for(int i=0; i < achievements.Length; i++) {
            UpdateAchievementUI(i);
        }
    }

    // Function to update the UI in the Achievement Menu based on the Achievement's state.
    public void UpdateAchievementUI(int id) {
        Image img = buttonIcons.transform.GetChild(id).GetComponent<Image>();
        if (GameManager.achievementData.GetAchievement(id).achieved) { img.sprite = achievementSprite[id]; } // Achievement is unlocked.
        else { img.sprite = lockedSprite; } // Achievement is locked.
    }
    
    public void DisplayAchievement(int id) {
        Image img = buttonIcons.transform.GetChild(id).GetComponent<Image>();
        achievementDisplay.transform.GetChild(0).GetComponent<Image>().sprite = img.sprite;
        // Achievement is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            achievementDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = "???";
        } else { // Achievement is unlocked, so show achievement information.
            achievementDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = achievements[id].name;
        } 
        
        // For certain achievements, display the associated tracking variable for clarity.
        string res = achievements[id].description;
        switch (id) {
            case 1: { // Lifetime Deliveries
                    res += " [" + GameManager.achievementData.GetLifetimeScore() + "]";
                    break; }
            case 3:
            case 4: { // Player Crashes
                    res += " [" +  GameManager.achievementData.GetPlayerCrashes() + "]";
                    break; }}
        achievementDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = res;
    }  

}