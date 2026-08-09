using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Database", menuName = "Scriptable Objects/Database")]
public class SO_Database : ScriptableObject {

    [SerializeField] private List<Obstacle> obstacles;
    [SerializeField] private List<Prop> props;
    [SerializeField] private List<Achievement_SO> achievements;
    [SerializeField] private List<Upgrade_SO> upgrades;
    [SerializeField] private List<Level_SO> levels;

    public List<Obstacle> GetObstacles() => obstacles;
    public List<Prop> GetProps() => props;
    public List<Achievement_SO> GetAchievements() => achievements;
    public List<Upgrade_SO> GetUpgrades() => upgrades;
    public List<Level_SO> GetLevels() => levels;
    
}
