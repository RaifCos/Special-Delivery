using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct Achievement {
        public string name, description;
        public bool achieved;

        public Achievement(string n, string d) {
            name = n;
            description = d;
            achieved = false;
        }
        
        public void SetAchieved(bool input) { achieved = input; }
    }

// Script to handle achievement tracking and the achievement menu UI.
public class AchievementManager : MonoBehaviour {

    [Header("Achievement Menu Variables")]
    public GameObject buttonIcons;
    public GameObject achievementDisplay;
    public Sprite lockedSprite;
    public Sprite[] achievementSprite;

    private readonly Achievement[] achievements = {
    new("EMPLOYEE OF THE MONTH", "complete 10 deliveries in a single game."),
    new("DUTIFUL DELIVER-ER", "complete 250 deliveries."),
    new("THE COMPLETE PACKAGE", "complete 50 deliveries in a single game."),
    new("CRASH", "crash 100 times."),
    new("KABOOM", "crash 1,000 times."),
    new("TIME TO SPARE", "accumulate 100 seconds on the game timer."),
    new("WELL NOW I'VE SEEN EVERYTHING", "encounter every obstacle."),
    new("NO SIGN OF LIFE", "destory all stop signs and street signs in a single game."),
    new("NO CONE ZONE", "destory all traffic cones in a single game."),
    new("TAKING OUT THE TRASH", "destory all bins in a single game."),
    new("HYDRANT HUNTER", "destory all fire hydrants in a single game."),
    new("PARKS AND WRECK", "destory all benches in a single game.") };

    // Variables used for tracking achievements.
    public int lifetimeDeliveries, playerCrashes;

    void Awake() {
        GameManager.achievementManager = this;
    }

    void Start() {
        // Load saved stats.
        lifetimeDeliveries = PlayerPrefs.GetInt("LifetimeScore", 0);
        playerCrashes = PlayerPrefs.GetInt("PlayerCrashes", 0);
        UpdateData(false);
    }

    public Achievement GetAchievement(int id) { return achievements[id]; }

    public int GetLifetimeScore() { return lifetimeDeliveries; }

    public int GetPlayerCrashes() { return playerCrashes; }

    // Function to denote an Achievement as completed, also saves the milestone and updates the correct UI.
    public void CompleteAchievement(int id) {
        // Only change if achievement has not yet been aquired or the player isn't in the tutorial.
        if (!achievements[id].achieved && GameManager.instance.GetDifficulty() != 0) {
            achievements[id].achieved = true;

            // Update save data to reflece achievement status.
            PlayerPrefs.SetInt(achievements[id].name, 1);
            PlayerPrefs.Save();

            GameManager.newsTextScroller.AddAchievementHeadline(achievements[id].name); // Create Headline to display in game.
        }
    }

    // Function to reset all Achievements and progress.
    public void UpdateData(bool updateUI) {
        for (int i = 0; i < achievements.Length; i++) {
            achievements[i].SetAchieved(PlayerPrefs.GetInt(achievements[i].name, 0) == 1);
            if (updateUI) { UpdateAchievementUI(i); }
        }
    }

    // Function that increments and saves progress on a given statistic. 
    public void IncreaseProgress(int id) {
        if (GameManager.instance.GetDifficulty() != 0) {
            switch (id) {
                case 0: { // Lifetime Deliveries
                        lifetimeDeliveries++;
                        PlayerPrefs.SetInt("LifetimeScore", lifetimeDeliveries);
                        PlayerPrefs.Save();
                        if (lifetimeDeliveries == 25) { 
                            GameManager.instance.SetShopProgress(true); 
                            GameManager.newsTextScroller.AddShopHeadline();
                        } if (lifetimeDeliveries == 250) { CompleteAchievement(1); }
                        break; }
                case 1: { // Player Crashes
                        playerCrashes++;
                        PlayerPrefs.SetInt("PlayerCrashes", playerCrashes);
                        PlayerPrefs.Save();
                        if (playerCrashes == 100) { CompleteAchievement(3); }
                        else if (playerCrashes == 1000) { CompleteAchievement(4); }
                        break; }
            }
        }
    }

        // Function to update the UI in the Achievement Menu based on the Achievement's state.
    public void UpdateAchievementUI(int id) {
        Image img = buttonIcons.transform.GetChild(id).GetComponent<Image>();
        if (GameManager.achievementManager.GetAchievement(id).achieved) { img.sprite = achievementSprite[id]; } // Achievement is unlocked.
        else { img.sprite = lockedSprite; } // Achievement is locked.
    }
    
    public void DisplayAchievement(int id) {
        Image img = buttonIcons.transform.GetChild(id).GetComponent<Image>();
        achievementDisplay.transform.GetChild(0).GetComponent<Image>().sprite = img.sprite;
        // Achievement is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            achievementDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = "???";
        } else { // Achievement is unlocked, so show achievement information.
            achievementDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = GameManager.achievementManager.GetAchievement(id).name;
        } 
        
        // For certain achievements, display the associated tracking variable for clarity.
        string res = GameManager.achievementManager.GetAchievement(id).description;
        switch (id) {
            case 1: { // Lifetime Deliveries
                    res += " [" + GameManager.achievementManager.GetLifetimeScore() + "]";
                    break; }
            case 3:
            case 4: { // Player Crashes
                    res += " [" +  GameManager.achievementManager.GetPlayerCrashes() + "]";
                    break; }}
        achievementDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = res;
    }  

}