using System.Linq;
using UnityEngine;

public class ObstacleData : MonoBehaviour {

    [SerializeField]
    private ObstaclePerm[] permObstacles;
    [SerializeField]
    private ObstacleTemp[] tempObstacles;

    private int[] lifetimePermObs, lifetimeTempObs, lifetimeProps;

    void Awake() { GameManager.obstacleData = this; ResetEncounters(); }

    void Start() {
        for (int i = 0; i < lifetimePermObs.Length; i++) { lifetimePermObs[i] = PlayerPrefs.GetInt("EncounterPerm" + i, 0); }
        for (int i = 0; i < lifetimeTempObs.Length; i++) { lifetimeTempObs[i] = PlayerPrefs.GetInt("EncounterTemp" + i, 0); }
        for (int i = 0; i < lifetimeProps.Length; i++) { lifetimeProps[i] = PlayerPrefs.GetInt("EncounterProp" + i, 0); }   
    }

    public ObstaclePerm[] GetPermObstacles() { return permObstacles; }
    public ObstacleTemp[] GetTempObstacles() { return tempObstacles; }
    
    // Function to increment the number of times an obstacle has been "encountered" (for achievement tracking).
    public void AddPermObstacleEncounter(int id) {
        lifetimeTempObs[id]++;
        PlayerPrefs.SetInt("EncounterTemp" + id, lifetimePermObs[id]);
        PlayerPrefs.Save();
        AchievementCheck();
    }

    public  void AddTempObstacleEncounter(int id) {
        lifetimePermObs[id]++;
        PlayerPrefs.SetInt("EncounterPerm" + id, lifetimeTempObs[id]);
        PlayerPrefs.Save();
        AchievementCheck();
    }
    
    public void AddPropEncounter(int id) {
        lifetimeProps[id]++;
        PlayerPrefs.SetInt("EncounterProp" + id, lifetimeProps[id]);
        PlayerPrefs.Save();
        AchievementCheck();
        CheckProps();
    }

    public void AchievementCheck() {
        if (!lifetimePermObs.Contains(0) && !lifetimeTempObs.Contains(0) && !lifetimeProps.Contains(0)) { GameManager.achievementData.CompleteAchievement(6); }
    }
    
    public void ResetEncounters() {
        lifetimePermObs = new int[permObstacles.Length];
        lifetimeTempObs = new int[tempObstacles.Length];
        lifetimeProps = new int[8];
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