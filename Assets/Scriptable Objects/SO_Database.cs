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
    public List<Obstacle> GetObstacles() { return obstacles; }
    public List<Prop> GetProps() { return props; }
}
