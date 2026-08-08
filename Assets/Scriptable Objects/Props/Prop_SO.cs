using UnityEngine;

[CreateAssetMenu(fileName = "Prop_SO", menuName = "Scriptable Objects/Prop")]
public class Prop_SO : ScriptableObject {
    public string internalName, externalName;
    [TextArea(3, 6)]
    public string description;
    public bool stackable; // Ignore Collisions with Props of the same type.
    public bool suspended; // Prop stays in place until hit.
    public bool isLit; // Prop is a light source that is broken when hit.
    public Sprite sprite; 
}
