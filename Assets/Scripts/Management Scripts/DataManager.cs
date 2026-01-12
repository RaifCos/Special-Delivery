using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    [SerializeField]
    private SO_Database database;
    private List<Obstacle> obstacles;
    private List<Prop> props;
    private List<Achievement_SO> achievements;
    private Dictionary<string, int> gameObs;
    private Dictionary<string, int> gameProps;
    private Dictionary<string, int> lifetimeObs, lifetimeProps;
    private readonly Dictionary<string, bool> achievementProgress = new();
    private int lifetimeDeliveries, playerCrashes;

    #region Static Data
    void Awake() { GameManager.dataManager = this; }

    void Start() {
        obstacles = database.GetObstacles();
        props = database.GetProps();
        achievements = database.GetAchievements();
        LoadEncounterData();
    }

    public List<Obstacle> GetObstacles() { return obstacles; }

    public Obstacle GetObstacle(string key) { return obstacles.Find(obs => obs.so.internalName == key); }
    public List<Prop> GetProps() { return props; }

    public Prop GetProp(string key) { return props.Find(prop => prop.so.internalName == key); }
    public List<Achievement_SO> GetAchievements() { return achievements; }
    public Achievement_SO GetAchievement(string key) { return achievements.Find(ach => ach.internalName == key); }

    #endregion 
    
    #region Obstacle/Prop Data
    private void LoadEncounterData() {
        lifetimeObs = new();
        foreach (var obs in obstacles) { 
            string name = obs.so.internalName;
            lifetimeObs[name] = PlayerPrefs.GetInt("EncounterObs_" + name, 0);
        }

        lifetimeProps = new();        
        foreach (var prop in props) { 
            string name = prop.so.internalName;
            lifetimeProps[name] = PlayerPrefs.GetInt("EncounterProp_" + name, 0); 
        }
    }

    public void AddObstacleEncounter(string key) {
        gameObs[key]++;
        if (lifetimeProps[key] == 1) GalleryCompletionCheck();
    }
    
    public void AddPropEncounter(string key) {
        lifetimeProps[key]++;
        if (lifetimeProps[key] == 1) GalleryCompletionCheck();
        CheckProps();
    }

    public void AddEncountersToTotal() {
        var keys = new List<string>(gameObs.Keys);
        foreach (var key in keys) { 
            lifetimeObs[key] += gameObs[key];
            PlayerPrefs.SetInt("EncounterObs_" + key, lifetimeObs[key]); 
        }

        keys = new List<string>(gameProps.Keys);
        foreach (var key in keys) { 
            lifetimeProps[key] += gameProps[key];
            PlayerPrefs.SetInt("EncounterProp_" + key, lifetimeProps[key]); 
        }

        PlayerPrefs.Save();
    }

    public bool CheckLimit(Obstacle obs) => gameObs[obs.so.internalName] < obs.so.limit;

    public void ResetGameEncounters() {
        gameObs = new();
        foreach (Obstacle obs in obstacles) { gameObs.Add(obs.so.internalName, 0); }

        gameProps = new();
        foreach (Prop prop in props) { gameProps.Add(prop.so.internalName, 0); }
    }

    private void GalleryCompletionCheck() {
        if (!lifetimeObs.ContainsValue(0) && !lifetimeProps.ContainsValue(0)) 
        { CompleteAchievement("galleryAll"); }
    }
    
    // Function to check if all props of a certain type have been destoyed (for achievement tracking).
    public void CheckProps() {
        if (GameObject.Find("stopSign") == null && GameObject.Find("streetSign") == null) { CompleteAchievement("destroySigns"); }
        if (GameObject.Find("cone") == null) { CompleteAchievement("destroyCones"); }
        if (GameObject.Find("bin") == null) { CompleteAchievement("destroyBins"); }
        if (GameObject.Find("hydrant") == null) { CompleteAchievement("destroyHydrants"); }
        if (GameObject.Find("bench") == null) { CompleteAchievement("destroyBenches"); }
    }
    #endregion

    #region Achievement Data
    public bool IsAchieved(string key) => achievementProgress[key];

    public int GetLifetimeScore() { return lifetimeDeliveries; }

    public int GetPlayerCrashes() { return playerCrashes; }

    // Function to denote an Achievement as completed.
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
    public void UpdateAchievementData() {
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
    #endregion
}
