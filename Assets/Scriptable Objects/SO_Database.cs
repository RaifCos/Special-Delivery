using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Database", menuName = "Scriptable Objects/Database")]
public class SO_Database : ScriptableObject {

    [SerializeField]
    private List<Achievement_SO> achievements;
    [SerializeField]
    private List<Obstacle> obstacles;
    [SerializeField]
    private List<Prop> props;

    public List<Achievement_SO> GetAchievements() { return achievements; }

    public Achievement_SO GetAchievement(string key) { return achievements.Find(ach => ach.internalName == key); }
    public List<Obstacle> GetObstacles() { return obstacles; }

    public Obstacle GetObstacle(string key) { return obstacles.Find(obs => obs.so.internalName == key); }
    public List<Prop> GetProps() { return props; }

    public Prop GetProp(string key) { return props.Find(prop => prop.so.internalName == key); }
}
