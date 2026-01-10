using UnityEngine;

[CreateAssetMenu(fileName = "ObstaclePerm_SO", menuName = "Scriptable Objects/Permanent Obstacle")]
public class ObstaclePerm_SO : ScriptableObject {
    public string intenalName, externalName;
    [TextArea(3, 6)]
    public string description, headline;
    public int limit;
}
