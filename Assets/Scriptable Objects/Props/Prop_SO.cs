using UnityEngine;

[CreateAssetMenu(fileName = "Prop_SO", menuName = "Scriptable Objects/Prop")]
public class Prop_SO : ScriptableObject {
    public string internalName, externalName;
    [TextArea(3, 6)]
    public string description;
    public bool includeGround;
    public Sprite sprite; 
}
