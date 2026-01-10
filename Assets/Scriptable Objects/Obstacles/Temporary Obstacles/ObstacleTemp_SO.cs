using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleTemp_SO", menuName = "Scriptable Objects/Temporary Obstacle")]
public class ObstacleTemp_SO : ScriptableObject {
    public string intenalName, externalName;
    [TextArea(3, 6)]
    public string description, headline;
}
