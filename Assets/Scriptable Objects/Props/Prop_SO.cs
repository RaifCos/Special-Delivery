using UnityEngine;

[CreateAssetMenu(fileName = "Prop_SO", menuName = "Scriptable Objects/Prop")]
public class Prop_SO : ScriptableObject {
    public string intenalName, externalName;
    [TextArea(3, 6)]
    public string description, headline;
    public bool includeGround;
}
