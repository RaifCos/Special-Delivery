using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level_SO", menuName = "Scriptable Objects/Level")]
public class Level_SO : ScriptableObject {
    public int levelNumber;
    public string internalName, externalName;
    [TextArea(3, 6)]
    public string description;
    [TextArea(3, 6)]
    public string openingHeadline;
    [TextArea(2, 6)]
    public string[] headlines;
    public List<Level_SO> unlocks; 
    public Sprite sprite; 
}
