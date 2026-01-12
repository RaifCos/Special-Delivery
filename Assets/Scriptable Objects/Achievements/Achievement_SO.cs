using UnityEngine;

[CreateAssetMenu(fileName = "Achievement_SO", menuName = "Scriptable Objects/Achievement")]
public class Achievement_SO : ScriptableObject {
    public string internalName, externalName;
    [TextArea(3, 6)]
    public string description;
}
