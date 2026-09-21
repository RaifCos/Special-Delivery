using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

#region Data Classes
[Serializable]
public class Data {
    [SerializeField] public Dictionary<string, int> lifetimeObs = new();
    [SerializeField] public Dictionary<string, int> lifetimeProps = new();
    [SerializeField] public Dictionary<PropGroup, int> lifetimePropGroups = new();
    [SerializeField] public Dictionary<string, bool> achievementProgress = new();
    [SerializeField] public Dictionary<string, bool> upgradeProgress = new();
    [SerializeField] public Dictionary<string, int> levelProgress = new();
    [SerializeField] public Dictionary<string, int> levelScores = new();
    [SerializeField] public bool[] stampCollection;
    public int lifetimeDeliveries, playerCrashes, bestScore, cash, stampCount = 0;
    public bool shopUnlocked = false;
    public List<string> cutsceneQueue = new();
}

public class ProgressData {
    public int galleryProgress = 0;
    public int achievementProgress = 0;
    public int upgradeProgress = 0;
    public int levelProgress = 0;
    public int totalProgress = 0;
    public int stampProgress = 0;
    public bool shopUnlocked = false;
    public bool isEmpty = true;
}

#endregion

public class DataManager : MonoBehaviour {

    #region Variables
    // Save Data 
    [SerializeField] private string jsonFileName;
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, jsonFileName) + GameManager.instance.GetSaveFile(); 

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
        if (File.Exists(SaveFilePath)) {
            string encryptedJson = File.ReadAllText(SaveFilePath);
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
        File.WriteAllText(SaveFilePath, encryptedJson);
    }

    private void LoadMissingData() {
        foreach (Obstacle obs in obstacles)
            data.lifetimeObs.TryAdd(obs.so.internalName, 0);

        foreach (Prop prop in props)
            data.lifetimeProps.TryAdd(prop.so.internalName, 0);

        foreach (PropGroup group in Enum.GetValues(typeof(PropGroup)))
            data.lifetimePropGroups.TryAdd(group, 0);

        foreach (Achievement_SO ach in achievements)
            data.achievementProgress.TryAdd(ach.internalName, false);

        foreach (Upgrade_SO up in upgrades)
            data.upgradeProgress.TryAdd(up.internalName, false);

        foreach (Level_SO level in levels) {
            data.levelProgress.TryAdd(level.internalName, 0);
            data.levelScores.TryAdd(level.internalName, 0);
        }

        data.stampCollection ??= new bool[levels.Count * 3];
    }

    public Data ResetData() {
        if (File.Exists(SaveFilePath)) { File.Delete(SaveFilePath); }
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

        foreach (PropGroup group in Enum.GetValues(typeof(PropGroup))) {
            defaultData.lifetimePropGroups[group] = 0;
        }

        foreach(Achievement_SO ach in achievements) {
            defaultData.achievementProgress[ach.internalName] = false;
        }

        foreach(Upgrade_SO up in upgrades) {
            defaultData.upgradeProgress[up.internalName] = false;
        }

        foreach (Level_SO level in levels) {
            defaultData.levelProgress[level.internalName] = 0;
            defaultData.levelScores[level.internalName] = 0;
        } defaultData.levelProgress["city"] = 1;

        defaultData.stampCollection = new bool[levels.Count * 3];

        return defaultData;
    }

    #endregion 
    
    #region Save File Data
    public ProgressData[] LoadSaveFiles() {
        ProgressData[] saveFileProgress = new ProgressData[3];
        int totalGallery      = obstacles.Count + props.Count;
        int totalStamps       = levels.Count * 3;
        int totalAchievements = achievements.Count;
        int totalUpgrades     = upgrades.Count;
        int totalItems        = totalGallery + totalAchievements + totalUpgrades + totalStamps;

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

            int stamps = 0;
            for (int k = 0; k < totalStamps; k++) { if (data.stampCollection[k]) stamps++; }

            saveFileProgress[i] = new ProgressData {
                levelProgress       = beatenLevels      > 0 ? Mathf.RoundToInt((float)beatenLevels / (levels.Count * 2) * 100) : 0,
                galleryProgress     = totalGallery      > 0 ? Mathf.RoundToInt((float)gallery  / totalGallery      * 100) : 0,
                achievementProgress = totalAchievements > 0 ? Mathf.RoundToInt((float)achieved / totalAchievements * 100) : 0,
                upgradeProgress     = totalUpgrades     > 0 ? Mathf.RoundToInt((float)upgraded  / totalUpgrades    * 100) : 0,
                stampProgress       = totalStamps       > 0 ? Mathf.RoundToInt((float)stamps  / totalStamps * 100) : 0,
                totalProgress       = totalItems        > 0 ? Mathf.RoundToInt((float)(gallery + achieved + upgraded + stamps + beatenLevels) / totalItems * 100) : 0,
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
            Level_SO currentLvl = GetLevel(key);
            foreach (Level_SO lvl in GetLevels()) { 
                if (lvl.unlockedBy.Contains(currentLvl))
                LevelUnlockCheck(lvl.internalName); 
            }
        }
    }

    public int GetLevelScore(string key) => data.levelScores[key];
    public void SetLevelScore(string key, int value) => data.levelScores[key] = value;
    public void IncrementLevelScore(string key) { 
        data.levelScores[key]++;
        int val = data.levelScores[key];

        if (GetLevelProgress(key) < 2 && val >= GetLevel(key).bossUnlockScore) {
            SetLevelProgress(key, 2);
            GameManager.newsTextScroller.AddBossUnlockHeadline();
            AddCutsceneToQueue("boss-" + key);
        }

        data.lifetimeDeliveries++;

        if (!IsShopUnlocked() && data.lifetimeDeliveries >= 25) { 
            SetShopProgress(true); 
            GameManager.newsTextScroller.AddShopUnlockHeadline();
            AddCutsceneToQueue("garage-unlock");
        }
        
        if (data.lifetimeDeliveries == 250) { CompleteAchievement("lifetime250"); }
    }

    public void LevelUnlockCheck(string key) {
        if (GetLevelProgress(key) != 0) return; // Ignore if Level is already unlocked
        Level_SO lvl = GetLevel(key);
        foreach (Level_SO previousLevel in lvl.unlockedBy) {
            // Don't Unlock Level if required levels haven't been beat.
            if (GetLevelProgress(previousLevel.internalName) < 3) return;
        } 
        
        AddCutsceneToQueue("level-" + key);
        SetLevelProgress(key, 1); 
    }

    public string LevelUnlockList(string key) {
        Level_SO lvl = GetLevel(key);
        List<Level_SO> list = lvl.unlockedBy;
        int count = list.Count;

        switch (count) {
            case 0:
                return "";
            case 1:
                return list[0].externalName;
            case 2:
                return list[0].externalName + " and " + list[1].externalName;
            default:
                string res = "";
                for (int i = 0; i < count - 1; i++) {
                    res += list[i].externalName + ", ";
                } res += "and " + list[count - 1].externalName;
                return res;
        }
    }

    #endregion

    #region Obstacle/Prop Data
    public void AddObstacleEncounter(string key) {
        data.lifetimeObs[key]++;
        if (data.lifetimeObs.GetValueOrDefault(key) == 1) GalleryCompletionCheck();
    }
    
    public void AddPropEncounter(string key) {
        data.lifetimeProps[key]++;
        if (data.lifetimeProps.GetValueOrDefault(key) == 1) GalleryCompletionCheck();
    }

    public void AddPropEncounter(string key, PropGroup group) {
        AddPropEncounter(key);
        AddGroupEncounter(group);
    }

    public void AddGroupEncounter(PropGroup group) {
        data.lifetimePropGroups[group]++;
        int count = data.lifetimePropGroups[group];

        switch (group) {
            case PropGroup.Benches:
                if (count == 100) { CompleteAchievement("destroyBenches"); }
                break;
            case PropGroup.Bins:
                if (count == 150) { CompleteAchievement("destroyBins"); }
                break;
            case PropGroup.Cones:
                if (count == 250) { CompleteAchievement("destroyCones"); }
                break;
            case PropGroup.Hydrants:
                if (count == 100) { CompleteAchievement("destroyHydrants"); }
                break;
            case PropGroup.Signs:
                if (count == 300) { CompleteAchievement("destroySigns"); }
                break;
        }
    }

    public int GetObstacleEncounters(string key) => data.lifetimeObs[key];

    public int GetPropEncounters(string key) => data.lifetimeProps[key];
    public int GetGroupEncounters(PropGroup group) => data.lifetimePropGroups[group];

    public Dictionary<PropGroup, int> GetAllGroupEncounters() => data.lifetimePropGroups;

    private void GalleryCompletionCheck() {
        if (!data.lifetimeObs.ContainsValue(0) && !data.lifetimeProps.ContainsValue(0)) 
        { CompleteAchievement("galleryAll"); }
    }

    #endregion

    #region Achievement Data

    public int GetLifetimeDeliveries() => data.lifetimeDeliveries; 
    public int GetPlayerCrashes() => data.playerCrashes;
    public Achievement_SO[] GetHidingAchievements(string key) => GetAchievement(key).hiddenBehind;

    public int AchievementState(string key) {
        if (data.achievementProgress[key]) return 2; // Achievement Unlocked.
        else foreach (Achievement_SO a in GetHidingAchievements(key)) {
            if (!data.achievementProgress[a.internalName]) return 0; // Achievement Hidden.
        } return 1; // Achievement Locked but Unhidden. 
    
    }

    // Function to denote an Achievement as completed.
    public void CompleteAchievement(string key) {
        // Only change if achievement has not yet been aquired or the player isn't in the tutorial.
        if (!data.achievementProgress[key] && GameManager.instance.GetDifficulty() != 0) {
            data.achievementProgress[key] = true;
            string name = achievements.Find(ach => ach.name == key).externalName;
            GameManager.newsTextScroller.AddAchievementHeadline(name); // Create Headline to display in game.
            AddCutsceneToQueue("achievement-" + key);
        }
    }

    public void IncreaseCrashCount() {
        if (GameManager.instance.GetDifficulty() != 0) {
            data.playerCrashes++;
            if (data.playerCrashes == 1000) { CompleteAchievement("crash1000"); }
            else if (data.playerCrashes == 10000) { CompleteAchievement("crash10000"); }
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

    public bool AllRequiredUpgradesUnlocked(string key) {
        Upgrade_SO up = GetUpgrade(key);
        foreach(Upgrade_SO req in up.requirements) {
            if (!IsUnlocked(req.internalName)) return false;
        } return true;
    }

    public string UpgradeUnLocklist(string key) {
        Upgrade_SO up = GetUpgrade(key);
        List<Upgrade_SO> list = up.requirements
            .Where(req => !IsUpgraded(req.internalName))
            .ToList();

        int count = list.Count;

        switch (count) {
            case 0:
                return "";
            case 1:
                return list[0].externalName;
            case 2:
                return list[0].externalName + " and " + list[1].externalName;
            default:
                string res = "";
                for (int i = 0; i < count - 1; i++) {
                    res += list[i].externalName + ", ";
                } res += "and " + list[count - 1].externalName;
                return res;
        }
    }

    #endregion

    #region High-Score Data
    public int GetBestScore() { return data.bestScore; }
    public void SetBestScore(int val) { data.bestScore = val; }
    #endregion

    #region Cutscene Data

    public void AddCutsceneToQueue(string name) => data.cutsceneQueue.Add(name);

    public string DequeueCutscene() {
        List<string> queue = data.cutsceneQueue;
        if (queue.Count == 0) return null;
        string res = data.cutsceneQueue[0];
        data.cutsceneQueue.RemoveAt(0);
        return res;
    }

    public bool CutscenesQueued() { return data.cutsceneQueue.Count > 0; }

    #endregion   
    #region Stamp Data

    public void StampCollected(int index) {
        if (IsStampCollected(index)) return;
        data.stampCollection[index] = true;
        data.stampCount++;
        
        switch (data.stampCount) {
            case 1:
                AddCutsceneToQueue("unlock-obstacleGallery");
                break;
            case 3:
                AddCutsceneToQueue("unlock-propGallery");
                break;
        }
    } 

    public bool IsStampCollected(int index) => data.stampCollection[index];

    public int GetStampCount() => data.stampCount;

    #endregion    
}