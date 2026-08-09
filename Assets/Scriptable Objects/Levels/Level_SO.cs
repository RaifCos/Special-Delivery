using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level_SO", menuName = "Scriptable Objects/Level")]
public class Level_SO : ScriptableObject {
    public int levelNumber;
    public string internalName, externalName;
    public List<Level_SO> unlocks; 
}
