using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class Data {
    public Dictionary<string, int> lifetimeObs = new();
    public Dictionary<string, int> lifetimeProps = new();
    public Dictionary<string, bool> achievementProgress = new();
    public Dictionary<string, bool> upgradeProgress = new();
    public int lifetimeDeliveries, playerCrashes, bestScore, cash = 0;
    public bool shopUnlocked = false;
}

public class DataManager : MonoBehaviour {

    #region Variables
    // Save Data 
    [SerializeField] private string jsonFileName;
    private string saveFilePath; 

    // Static Variables
    [SerializeField] private SO_Database database;
    private static List<Obstacle> obstacles;
    private static List<Prop> props;
    private static List<Achievement_SO> achievements;
    private static List<Upgrade_SO> upgrades;

    Data data = new();

    // Obstacle Variables
    private Dictionary<string, int> gameObs = new();
    private Dictionary<string, int> gameProps = new();
    #endregion

    #region Scriptable Object Methods
    void Awake() { 
        GameManager.dataManager = this;
        obstacles = database.GetObstacles();
        props = database.GetProps();
        achievements = database.GetAchievements();
        upgrades = database.GetUpgrades();
        saveFilePath = Path.Combine(Application.persistentDataPath, jsonFileName);
        LoadData();
    }

    public List<Obstacle> GetObstacles() { return obstacles; }

    public Obstacle GetObstacle(string key) { return obstacles.Find(obs => obs.so.internalName == key); }
    public List<Prop> GetProps() { return props; }

    public Prop GetProp(string key) { return props.Find(prop => prop.so.internalName == key); }
    public List<Achievement_SO> GetAchievements() { return achievements; }
    public Achievement_SO GetAchievement(string key) { return achievements.Find(ach => ach.internalName == key); }
    public List<Upgrade_SO> GetUpgrades() { return upgrades; }
    public Upgrade_SO GetUpgrade(string key) { return upgrades.Find(up => up.internalName == key); }
    #endregion

    #region Save Data
    public void LoadData() {
        if (File.Exists(saveFilePath)) {
            string encryptedJson = File.ReadAllText(saveFilePath);
            string json = DataEncryption.Decrypt(encryptedJson);
            data = JsonConvert.DeserializeObject<Data>(json);
        } else {
            data = GameManager.dataManager.DefaultData();
            SaveData(); 
        }
    }

    public void SaveData() {
        string json = JsonConvert.SerializeObject(data, Formatting.None);
        string encryptedJson = DataEncryption.Encrypt(json);
        File.WriteAllText(saveFilePath, encryptedJson);
    }

    public Data ResetData() {
        Data data = GameManager.dataManager.DefaultData();
        SaveData();
        return data;
    }

    public Data DefaultData() {
        Data defaultData = new();
        foreach(Obstacle obs in obstacles) {
            Debug.Log(obs.so.internalName);
            defaultData.lifetimeObs[obs.so.internalName] = 0;
        }

        foreach(Prop prop in props) {
            defaultData.lifetimeProps[prop.so.internalName] = 0;
        }

        foreach(Achievement_SO ach in achievements) {
            defaultData.achievementProgress[ach.internalName] = false;
        }

        foreach(Upgrade_SO up in upgrades) {
            defaultData.upgradeProgress[up.internalName] = false;
        }

        return defaultData;
    }

    #endregion 
    
    #region Obstacle/Prop Data
    public void AddObstacleEncounter(string key) {
        gameObs[key] = gameObs.GetValueOrDefault(key) + 1;
        if (data.lifetimeProps.GetValueOrDefault(key) == 1) GalleryCompletionCheck();
    }
    
    public void AddPropEncounter(string key) {
        gameProps[key] = gameProps.GetValueOrDefault(key) + 1;
        if (data.lifetimeProps.GetValueOrDefault(key) == 1) GalleryCompletionCheck();
        CheckProps();
    }

    public int GetObstacleEncounters(string key) => data.lifetimeObs[key];

    public int GetPropEncounters(string key) => data.lifetimeProps[key];

    public void AddEncountersToTotal() {
        var keys = new List<string>(gameObs.Keys);
        foreach (var key in keys) { 
            data.lifetimeObs[key] += gameObs[key];
        }

        keys = new List<string>(gameProps.Keys);
        foreach (var key in keys) { 
            data.lifetimeProps[key] += gameProps[key];
        }
    }

    public bool CheckLimit(Obstacle obs) => gameObs.GetValueOrDefault(obs.so.internalName) < obs.so.limit;

    public void ResetGameEncounters() {
        gameObs = new();
        foreach (Obstacle obs in obstacles) { gameObs.Add(obs.so.internalName, 0); }

        gameProps = new();
        foreach (Prop prop in props) { gameProps.Add(prop.so.internalName, 0); }
    }

    private void GalleryCompletionCheck() {
        if (!data.lifetimeObs.ContainsValue(0) && !data.lifetimeProps.ContainsValue(0)) 
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

    public int GetLifetimeDeliveries() => data.lifetimeDeliveries; 
    public int GetPlayerCrashes() => data.playerCrashes;

    public bool IsAchieved(string key) => data.achievementProgress[key];

    // Function to denote an Achievement as completed.
    public void CompleteAchievement(string key) {
        // Only change if achievement has not yet been aquired or the player isn't in the tutorial.
        if (!data.achievementProgress[key] && GameManager.instance.GetDifficulty() != 0) {
            data.achievementProgress[key] = true;
            string name = achievements.Find(ach => ach.name == key).externalName;
            GameManager.newsTextScroller.AddAchievementHeadline(name); // Create Headline to display in game.
        }
    }

    // Function that increments and saves progress on a given statistic. 
    public void IncreaseProgress(int id) {
        if (GameManager.instance.GetDifficulty() != 0) {
            switch (id) {
                case 0: { // Lifetime Deliveries
                        data.lifetimeDeliveries++;
                        if (data.lifetimeDeliveries == 25) { 
                            GameManager.dataManager.SetShopProgress(true); 
                            GameManager.newsTextScroller.AddShopHeadline();
                        } if (data.lifetimeDeliveries == 250) { CompleteAchievement("lifetime250"); }
                        break; }
                case 1: { // Player Crashes
                        data.playerCrashes++;
                        if (data.playerCrashes == 500) { CompleteAchievement("crash500"); }
                        else if (data.playerCrashes == 1000) { CompleteAchievement("crash1000"); }
                        break; }
            }
        }
    }
    #endregion

    #region Cash Data

    public int GetCash() => data.cash;
    
    public void SetCash(int input) { data.cash = input; }

    public void CashTransaction(int amount) { 
        data.cash += amount;
        if (data.cash > 1000000) { data.cash = 1000000; }    
        else if (data.cash < 0) { data.cash = 0; }
    }
    
    public bool CanAfford(int amount) => amount < data.cash;
    #endregion

    #region  Upgrade Data

    public bool IsShopUnlocked() { return data.shopUnlocked; }

    public void SetShopProgress(bool input) { data.shopUnlocked = input; }

    public bool IsUpgraded(string key) => data.upgradeProgress[key];

    public bool IsUnlocked(string key) {
        Upgrade_SO upgrade = GetUpgrade(key);
        foreach (Upgrade_SO up in upgrade.requirements) {
            if ( !IsUpgraded(up.internalName) ) { return false; }
        } return true;
    }

    public void BuyUpgrade() {
        string key = GameManager.garageMenuManager.GetListed();
        if (IsUnlocked(key) && !IsUpgraded(key)) {
            Upgrade_SO upgrade = GetUpgrade(key);
            if (CanAfford(upgrade.cost)) {
                CashTransaction(-upgrade.cost);
                ActivateUpgrade(key);
                GameManager.garageMenuManager.UpdateMenu();
                SaveData();
            }
        }
    }

    public void ActivateUpgrade(string key) { data.upgradeProgress[key] = true; }
    #endregion

    #region High-Score Data
    public int GetBestScore() { return data.bestScore; }
    public void SetBestScore(int val) { data.bestScore = val; }
    #endregion
}