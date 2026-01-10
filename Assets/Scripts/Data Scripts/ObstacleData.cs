using System.Linq;
using UnityEngine;

public class ObstacleData : MonoBehaviour
{
    ObstaclePerm[] permObstacles;
    ObstacleTemp[] tempObstacles;

    private int[] lifetimeObstacleCount, lifetimePropCount;

    void Awake() { GameManager.obstacleData = this; ResetEncounters(); }

    void Start() {
        for (int i = 0; i < lifetimeObstacleCount.Length; i++) {
            lifetimeObstacleCount[i] = PlayerPrefs.GetInt("EncounterO" + i, 0);
        }
        for (int i = 0; i < lifetimePropCount.Length; i++) {
            lifetimePropCount[i] = PlayerPrefs.GetInt("EncounterP" + i, 0);
        }   
    }

    public ObstaclePerm[] GetPermObstacles() { return permObstacles; }
    public ObstacleTemp[] GetTempObstacles() { return tempObstacles; }
    
    // Function to increment the number of times an obstacle has been "encountered" (for achievement tracking).
    public  void AddObstacleEncounter(int id) {
        lifetimeObstacleCount[id]++;
        PlayerPrefs.SetInt("EncounterO" + id, lifetimeObstacleCount[id]);
        PlayerPrefs.Save();
        if (!lifetimeObstacleCount.Contains(0) && !lifetimePropCount.Contains(0)) { GameManager.achievementData.CompleteAchievement(6); }
    }
    
    public void AddPropEncounter(int id) {
        lifetimePropCount[id]++;
        PlayerPrefs.SetInt("EncounterP" + id, lifetimePropCount[id]);
        PlayerPrefs.Save();
        if (!lifetimeObstacleCount.Contains(0) && !lifetimePropCount.Contains(0)) { GameManager.achievementData.CompleteAchievement(6); }
        CheckProps();
    }
    
    public void ResetEncounters() {
        lifetimeObstacleCount = new int[permObstacles.Length + tempObstacles.Length];
        lifetimePropCount = new int[8];
    }
    
    // Corountine to check if all props of a certain type have been destoyed (for achievement tracking).
    public void CheckProps() {
        if (GameObject.Find("Stop Sign") == null && GameObject.Find("Street Sign") == null) { GameManager.achievementData.CompleteAchievement(7); }
        if (GameObject.Find("Cone") == null) { GameManager.achievementData.CompleteAchievement(8); }
        if (GameObject.Find("Bin") == null) { GameManager.achievementData.CompleteAchievement(9); }
        if (GameObject.Find("Hydrant") == null) { GameManager.achievementData.CompleteAchievement(10); }
        if (GameObject.Find("Bench") == null) { GameManager.achievementData.CompleteAchievement(11); }
    }
}