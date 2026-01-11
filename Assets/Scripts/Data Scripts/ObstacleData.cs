using UnityEngine;
using System.Collections.Generic;

public class ObstacleData : MonoBehaviour {

    [SerializeField]
    private ObstaclePerm[] permObstacles;
    [SerializeField]
    private ObstacleTemp[] tempObstacles;
    [SerializeField]
    private Prop[] props;

    private Dictionary<string, int> gameTempObs, gamePermObs, gameProps, lifetimeTempObs, lifetimePermObs, lifetimeProps;

    void Awake() { GameManager.obstacleData = this; ResetLifetimeEncounters(); }

    void Start() {
        foreach (var pair in lifetimePermObs) { lifetimePermObs[pair.Key] = PlayerPrefs.GetInt("EncounterPerm_" + pair.Key, 0); }
        foreach (var pair in lifetimeTempObs) { lifetimeTempObs[pair.Key] = PlayerPrefs.GetInt("EncounterTemp_" + pair.Key, 0); }
        foreach (var pair in lifetimeProps) { lifetimeProps[pair.Key] = PlayerPrefs.GetInt("EncounterProp_" + pair.Key, 0); }  
    }

    public ObstaclePerm[] GetPermObstacles() { return permObstacles; }
    public ObstacleTemp[] GetTempObstacles() { return tempObstacles; }
    
    public void AddPermObstacleEncounter(string key) {
        gamePermObs[key]++;
        AchievementCheck();
    }

    public void AddTempObstacleEncounter(string key) {
        gameTempObs[key]++;
        AchievementCheck();
    }
    
    public void AddPropEncounter(string key) {
        lifetimeProps[key]++;
        AchievementCheck();
        CheckProps();
    }

    public void AddEncountersToTotal() {
        foreach (var pair in lifetimePermObs) { 
            lifetimePermObs[pair.Key] += gamePermObs[pair.Key];
            PlayerPrefs.SetInt("EncounterPerm_" + pair.Key, lifetimePermObs[pair.Key]); 
        }

        foreach (var pair in lifetimeTempObs) { 
            lifetimeTempObs[pair.Key] += gameTempObs[pair.Key];
            PlayerPrefs.SetInt("EncounterTemp_" + pair.Key, lifetimeTempObs[pair.Key]); 
        }

        foreach (var pair in lifetimeProps) { 
            lifetimeProps[pair.Key] += gameProps[pair.Key];
            PlayerPrefs.SetInt("EncounterProp_" + pair.Key, lifetimeProps[pair.Key]); 
        }

        PlayerPrefs.Save();
    }

    public bool CheckLimit(ObstaclePerm obs) {
        return gamePermObs[obs.so.intenalName] < obs.so.limit;
    }

    public void ResetGameEncounters() {
        gamePermObs = new();
        foreach(ObstaclePerm obs in permObstacles) { gamePermObs.Add(obs.so.intenalName, 0); }

        gameTempObs = new();
        foreach(ObstacleTemp obs in tempObstacles) { gameTempObs.Add(obs.so.intenalName, 0); }

        gameProps = new();
        foreach(ObstacleTemp obs in tempObstacles) { gameTempObs.Add(obs.so.intenalName, 0); }
    }

    public void ResetLifetimeEncounters() {
        lifetimePermObs = new();
        foreach(ObstaclePerm obs in permObstacles) { lifetimePermObs.Add(obs.so.intenalName, 0); }

        lifetimeTempObs = new();
        foreach(ObstacleTemp obs in tempObstacles) { lifetimeTempObs.Add(obs.so.intenalName, 0); }

        lifetimeProps = new();
        foreach(Prop prop in props) { lifetimeTempObs.Add(prop.so.intenalName, 0); }
    }

    private void AchievementCheck() {
        if (!lifetimePermObs.ContainsValue(0)
        && !lifetimeTempObs.ContainsValue(0)
        && !lifetimeProps.ContainsValue(0)) 
        { GameManager.achievementData.CompleteAchievement(6); }
    }
    
    // Function to check if all props of a certain type have been destoyed (for achievement tracking).
    public void CheckProps() {
        if (GameObject.Find("Stop Sign") == null && GameObject.Find("Street Sign") == null) { GameManager.achievementData.CompleteAchievement(7); }
        if (GameObject.Find("Cone") == null) { GameManager.achievementData.CompleteAchievement(8); }
        if (GameObject.Find("Bin") == null) { GameManager.achievementData.CompleteAchievement(9); }
        if (GameObject.Find("Hydrant") == null) { GameManager.achievementData.CompleteAchievement(10); }
        if (GameObject.Find("Bench") == null) { GameManager.achievementData.CompleteAchievement(11); }
    }
}