using UnityEngine;
using System.Collections.Generic;

public class ObstacleData : MonoBehaviour {

    [SerializeField]
    private List<Obstacle> obstacles;
    [SerializeField]
    private Prop[] props;

    private Dictionary<string, int> gameObs, gameProps, lifetimeObs, lifetimeProps;

    void Awake() { GameManager.obstacleData = this; ResetLifetimeEncounters(); }

    void Start() {
        foreach (var pair in lifetimeObs) { lifetimeObs[pair.Key] = PlayerPrefs.GetInt("Encounter_" + pair.Key, 0); }
        foreach (var pair in lifetimeProps) { lifetimeProps[pair.Key] = PlayerPrefs.GetInt("EncounterProp_" + pair.Key, 0); }  
    }

    public List<Obstacle> GetObstacles() { return obstacles; }

    public Obstacle GetObstacle(string key) {
        foreach (Obstacle obs in obstacles) {
        if (obs.so.intenalName == key) { return obs; }
        } return null;
    }

    public Prop[] GetProps() { return props; }

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
        foreach (var pair in lifetimeObs) { 
            lifetimeObs[pair.Key] += gameObs[pair.Key];
            PlayerPrefs.SetInt("EncounterPerm_" + pair.Key, lifetimeObs[pair.Key]); 
        }

        foreach (var pair in lifetimeProps) { 
            lifetimeProps[pair.Key] += gameProps[pair.Key];
            PlayerPrefs.SetInt("EncounterProp_" + pair.Key, lifetimeProps[pair.Key]); 
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
        if (GameObject.Find("Stop Sign") == null && GameObject.Find("Street Sign") == null) { GameManager.achievementData.CompleteAchievement(7); }
        if (GameObject.Find("Cone") == null) { GameManager.achievementData.CompleteAchievement(8); }
        if (GameObject.Find("Bin") == null) { GameManager.achievementData.CompleteAchievement(9); }
        if (GameObject.Find("Hydrant") == null) { GameManager.achievementData.CompleteAchievement(10); }
        if (GameObject.Find("Bench") == null) { GameManager.achievementData.CompleteAchievement(11); }
    }
}