using UnityEngine;
using System.Collections.Generic;

// Script to handle achievement tracking and the achievement menu UI.
public class AchievementData : MonoBehaviour {

    private List<Achievement_SO> achievements;
    private Dictionary<string, bool> achievementProgress = new();

    // Variables used for tracking achievements.
    public int lifetimeDeliveries, playerCrashes;

    void Awake() {
        GameManager.achievementData = this;
    }

    void Start() {
        achievements = GameManager.database.GetAchievements();
        // Load saved stats.
        lifetimeDeliveries = PlayerPrefs.GetInt("LifetimeScore", 0);
        playerCrashes = PlayerPrefs.GetInt("PlayerCrashes", 0);
        foreach(Achievement_SO ach in achievements) {
            achievementProgress.Add(ach.internalName, PlayerPrefs.GetInt("Achievement_" + ach.internalName, 0) == 1);
        }
    }

    public bool IsAchieved(string key) { return achievementProgress[key]; }

    public int GetLifetimeScore() { return lifetimeDeliveries; }

    public int GetPlayerCrashes() { return playerCrashes; }

    // Function to denote an Achievement as completed, also saves the milestone and updates the correct UI.
    public void CompleteAchievement(string key) {
        // Only change if achievement has not yet been aquired or the player isn't in the tutorial.
        if (!achievementProgress[key] && GameManager.instance.GetDifficulty() != 0) {
            achievementProgress[key] = true;
            // Update save data to reflece achievement status.
            PlayerPrefs.SetInt("Achievement_" + key, 1);
            PlayerPrefs.Save();
            string name = achievements.Find(ach => ach.name == key).externalName;
            GameManager.newsTextScroller.AddAchievementHeadline(name); // Create Headline to display in game.
        }
    }

    // Function to reset all Achievements and progress.
    public void UpdateData() {
        var keys = new List<string>(achievementProgress.Keys);
        foreach (var key in keys) {
            achievementProgress[key] = PlayerPrefs.GetInt("Achievement_" + key, 0) == 1;
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
                        } if (lifetimeDeliveries == 250) { CompleteAchievement("lifetime250"); }
                        break; }
                case 1: { // Player Crashes
                        playerCrashes++;
                        PlayerPrefs.SetInt("PlayerCrashes", playerCrashes);
                        PlayerPrefs.Save();
                        if (playerCrashes == 100) { CompleteAchievement("crash100"); }
                        else if (playerCrashes == 1000) { CompleteAchievement("crash1000"); }
                        break; }
            }
        }
    }
}