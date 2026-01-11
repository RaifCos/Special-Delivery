using UnityEngine;
using System.Collections.Generic;

public class ObstacleData : MonoBehaviour {

    [SerializeField]
    private List<Obstacle> obstacles;
    [SerializeField]
    private List<Prop> props;

    private Dictionary<string, int> gameObs, gameProps, lifetimeObs, lifetimeProps;

    void Awake() { GameManager.obstacleData = this; ResetLifetimeEncounters(); }

    void Start() {
        var keys = new List<string>(lifetimeObs.Keys);
        foreach (var key in keys) { lifetimeObs[key] = PlayerPrefs.GetInt("EncounterObs_" + key, 0); }

        keys = new List<string>(lifetimeObs.Keys);
        foreach (var key in keys) { lifetimeProps[key] = PlayerPrefs.GetInt("EncounterProp_" + key, 0); }
    }

    public List<Obstacle> GetObstacles() { return obstacles; }

    public Obstacle GetObstacle(string key) {
        foreach (Obstacle obs in obstacles) {
        if (obs.so.intenalName == key) { return obs; }
        } return null;
    }

    public List<Prop> GetProps() { return props; }

    public Prop GetProp(string key) {
        foreach (Prop prop in props) {
        if (prop.so.intenalName == key) { return prop; }
        } return null;
    }
    
    public void AddObstacleEncounter(string key) {
        gameObs[key]++;
        AchievementCheck();
    }
    
    public void AddPropEncounter(string key) {
        lifetimeProps[key]++;
        AchievementCheck();
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

    public bool CheckLimit(Obstacle obs) {
        return gameObs[obs.so.intenalName] < obs.so.limit;
    }

    public void ResetGameEncounters() {
        gameObs = new();
        foreach(Obstacle obs in obstacles) { gameObs.Add(obs.so.intenalName, 0); }

        gameProps = new();
        foreach(Prop prop in props) { gameProps.Add(prop.so.intenalName, 0); }
    }

    public void ResetLifetimeEncounters() {
        lifetimeObs = new();
        foreach(Obstacle obs in obstacles) { lifetimeObs.Add(obs.so.intenalName, 0); }

        lifetimeProps = new();
        foreach(Prop prop in props) { lifetimeProps.Add(prop.so.intenalName, 0); }
    }

    private void AchievementCheck() {
        if (!lifetimeObs.ContainsValue(0)
        && !lifetimeProps.ContainsValue(0)) 
        { GameManager.achievementData.CompleteAchievement(6); }
    }
    
    // Function to check if all props of a certain type have been destoyed (for achievement tracking).
    public void CheckProps() {
        if (GameObject.Find("stopSign") == null && GameObject.Find("streetSign") == null) { GameManager.achievementData.CompleteAchievement(7); }
        if (GameObject.Find("cone") == null) { GameManager.achievementData.CompleteAchievement(8); }
        if (GameObject.Find("bin") == null) { GameManager.achievementData.CompleteAchievement(9); }
        if (GameObject.Find("hydrant") == null) { GameManager.achievementData.CompleteAchievement(10); }
        if (GameObject.Find("bench") == null) { GameManager.achievementData.CompleteAchievement(11); }
    }
}