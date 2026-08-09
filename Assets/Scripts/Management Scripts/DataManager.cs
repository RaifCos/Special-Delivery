using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEditor.TestTools.CodeCoverage;

#region Data Classes
[System.Serializable]
public class Data {
    public Dictionary<string, int> lifetimeObs = new();
    public Dictionary<string, int> lifetimeProps = new();
    public Dictionary<string, bool> achievementProgress = new();
    public Dictionary<string, bool> upgradeProgress = new();
    public Dictionary<string, int> levelProgress = new();
    public Dictionary<string, int> levelScores = new();
    public int lifetimeDeliveries, playerCrashes, bestScore, cash = 0;
    public bool shopUnlocked = false;
}

public class ProgressData {
    public int galleryProgress = 0;
    public int achievementProgress = 0;
    public int upgradeProgress = 0;
    public int levelProgress = 0;
    public int totalProgress = 0;
    public bool shopUnlocked = false;
    public bool isEmpty = true;
}
#endregion

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
    private static List<Level_SO> levels;

    Data data = new();

    #endregion

    #region Scriptable Object Methods
    void Awake() { 
        GameManager.dataManager = this;
        obstacles = database.GetObstacles();
        props = database.GetProps();
        achievements = database.GetAchievements();
        upgrades = database.GetUpgrades();
        levels = database.GetLevels();
        saveFilePath = Path.Combine(Application.persistentDataPath, jsonFileName) + GameManager.instance.GetSaveFile();
        LoadData();
    }

    public List<Obstacle> GetObstacles() => obstacles; 
    public Obstacle GetObstacle(string key) { return obstacles.Find(obs => obs.so.internalName == key); }
    public List<Prop> GetProps() => props; 
    public Prop GetProp(string key) { return props.Find(prop => prop.so.internalName == key); }
    public List<Achievement_SO> GetAchievements() => achievements; 
    public Achievement_SO GetAchievement(string key) { return achievements.Find(ach => ach.internalName == key); }
    public List<Upgrade_SO> GetUpgrades() => upgrades; 
    public Upgrade_SO GetUpgrade(string key) { return upgrades.Find(up => up.internalName == key); }
    public List<Level_SO> GetLevels() => levels;
    public Level_SO GetLevel(string key)  { return levels.Find(lvl => lvl.internalName == key); }
    #endregion

    #region Save Data
    public void LoadData() {
        if (File.Exists(saveFilePath)) {
            string encryptedJson = File.ReadAllText(saveFilePath);
            string json = DataEncryption.Decrypt(encryptedJson);
            data = JsonConvert.DeserializeObject<Data>(json);
            LoadMissingData(); 
        } else {
            data = DefaultData(); // load defaults into memory only, don't save
        }
    }

    public void SaveData() {
        string json = JsonConvert.SerializeObject(data, Formatting.None);
        string encryptedJson = DataEncryption.Encrypt(json);
        File.WriteAllText(saveFilePath, encryptedJson);
    }

    private void LoadMissingData() {
        foreach (Obstacle obs in obstacles)
            data.lifetimeObs.TryAdd(obs.so.internalName, 0);

        foreach (Prop prop in props)
            data.lifetimeProps.TryAdd(prop.so.internalName, 0);

        foreach (Achievement_SO ach in achievements)
            data.achievementProgress.TryAdd(ach.internalName, false);

        foreach (Upgrade_SO up in upgrades)
            data.upgradeProgress.TryAdd(up.internalName, false);

        foreach (Level_SO level in levels) {
            data.levelProgress.TryAdd(level.internalName, 0);
            data.levelScores.TryAdd(level.internalName, 0);
        }
    }

    public Data ResetData() {
        if (File.Exists(saveFilePath)) { File.Delete(saveFilePath); }
        data = DefaultData();
        return data;
    }

    private Data DefaultData() {
        Data defaultData = new();
        foreach(Obstacle obs in obstacles) {
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

        foreach(Level_SO level in levels) {
            defaultData.levelProgress[level.internalName] = 0;
        } defaultData.levelProgress["city"] = 1;

        return defaultData;
    }

    #endregion 
    
    #region Save File Data
    public ProgressData[] LoadSaveFiles() {
        ProgressData[] saveFileProgress = new ProgressData[3];
        int totalGallery      = obstacles.Count + props.Count;
        int totalAchievements = achievements.Count;
        int totalUpgrades     = upgrades.Count;
        int totalItems        = totalGallery + totalAchievements + totalUpgrades;

        for (int i = 0; i < 3; i++) {
            string path = Path.Combine(Application.persistentDataPath, jsonFileName) + i;

            if (!File.Exists(path)) {
                saveFileProgress[i] = new ProgressData();
                continue;
            }

            string encryptedJson = File.ReadAllText(path);
            string json          = DataEncryption.Decrypt(encryptedJson);
            Data   data          = JsonConvert.DeserializeObject<Data>(json);

            // Check Gallery Progress
            int gallery = 0;
            foreach (Obstacle obs in obstacles)
                if (data.lifetimeObs.GetValueOrDefault(obs.so.internalName) > 0) gallery++;

            foreach (Prop prop in props)
                if (data.lifetimeProps.GetValueOrDefault(prop.so.internalName) > 0) gallery++;

            // Check Achievement Progress
            int achieved = 0;
            foreach (Achievement_SO ach in achievements)
                if (data.achievementProgress.GetValueOrDefault(ach.internalName)) achieved++;

            // Check Shop Progress
            int upgraded = 0;
            foreach (Upgrade_SO up in upgrades)
                if (data.upgradeProgress.GetValueOrDefault(up.internalName)) upgraded++;

            // Check Level Progress (Point for Each Boss Unlocked/Beaten)
            int beatenLevels = 0;
            foreach (Level_SO level in levels) {
                if (data.levelProgress.GetValueOrDefault(level.internalName) > 1) beatenLevels++;
                if (data.levelProgress.GetValueOrDefault(level.internalName) > 2) beatenLevels++;
            }

            saveFileProgress[i] = new ProgressData {
                levelProgress       = beatenLevels      > 0 ? Mathf.RoundToInt((float)beatenLevels / (levels.Count * 2) * 100) : 0,
                galleryProgress     = totalGallery      > 0 ? Mathf.RoundToInt((float)gallery  / totalGallery     * 100) : 0,
                achievementProgress = totalAchievements > 0 ? Mathf.RoundToInt((float)achieved / totalAchievements * 100) : 0,
                upgradeProgress     = totalUpgrades     > 0 ? Mathf.RoundToInt((float)upgraded  / totalUpgrades    * 100) : 0,
                totalProgress       = totalItems        > 0 ? Mathf.RoundToInt((float)(gallery + achieved + upgraded + beatenLevels) / totalItems * 100) : 0,
                shopUnlocked        = data.shopUnlocked,
                isEmpty             = false
            };
        } return saveFileProgress;
    }

    #endregion

    #region Level Data
    // 0 - Level Locked
    // 1 - Level Unlocked
    // 2 - Boss Unlocked 
    // 3 - Boss Beaten 

    public int GetLevelProgress(string key) => data.levelProgress[key];
    public void SetLevelProgress(string key, int value) { 
        data.levelProgress[key] = value; 
        if (value == 3) { // If Level is Completed, Unlock the Next.
            Level_SO lvl = GetLevel(key);
            foreach (Level_SO unlockedLvl in lvl.unlocks) {
                SetLevelProgress(unlockedLvl.internalName, 1);
            }
        }
    }

    public int GetLevelScore(string key) => data.levelScores[key];
    public void SetLevelScore(string key, int value) => data.levelScores[key] = value;

    #endregion

    #region Obstacle/Prop Data
    public void AddObstacleEncounter(string key) {
        data.lifetimeObs[key]++;
        if (data.lifetimeObs.GetValueOrDefault(key) == 1) GalleryCompletionCheck();
    }
    
    public void AddPropEncounter(string key) {
        data.lifetimeProps[key]++;
        if (data.lifetimeProps.GetValueOrDefault(key) == 1) GalleryCompletionCheck();
        CheckProps();
    }

    public int GetObstacleEncounters(string key) => data.lifetimeObs[key];

    public int GetPropEncounters(string key) => data.lifetimeProps[key];

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
                            GameManager.newsTextScroller.AddShopUnlockHeadline();
                        if (data.lifetimeDeliveries == 50) {
                            GameManager.dataManager.SetLevelProgress("city", 2); 
                            GameManager.newsTextScroller.AddBossUnlockHeadline();
                        }
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
                GameManager.garageMenuManager.UpdateMenu(true);
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