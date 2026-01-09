using System;
using UnityEngine;

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
            if (updateUI) { GameManager.mainMenuManager.UpdateAchievementUI(i); }
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
                        if (lifetimeDeliveries == 250) { CompleteAchievement(1); }
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
}