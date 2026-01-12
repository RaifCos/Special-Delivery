using UnityEngine;

[CreateAssetMenu(fileName = "Obstacle_SO", menuName = "Scriptable Objects/Obstacle")]
public class Obstacle_SO : ScriptableObject {
    public string internalName, externalName;
    [TextArea(3, 6)]
    public string description, headline;
    public int limit;
}
