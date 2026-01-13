using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade_SO", menuName = "Scriptable Objects/Upgrade")]
public class Upgrade_SO : ScriptableObject {
    public string internalName, externalName;
    [TextArea(3, 6)]
    public string description;
    public int cost;
    public Sprite sprite;
    public List<Upgrade_SO> requirements; 
}
