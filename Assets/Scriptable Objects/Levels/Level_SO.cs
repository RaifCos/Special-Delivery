using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level_SO", menuName = "Scriptable Objects/Level")]
public class Level_SO : ScriptableObject {
    public int levelNumber;
    public string internalName, externalName;
    [TextArea(3, 6)]
    public string description;
    [TextArea(3, 6)]
    public string openingHeadline; // Headline that is used when a shift begins. 
    [TextArea(2, 6)] 
    public string[] headlines; // Generic Headlines that is used during gameplay.
    public int bossUnlockScore; // The Number of Parcels needed to unlock the boss.
    public List<Level_SO> unlockedBy; // ALl the Levels that must be completed fot this Level to be unlocked.
    public Sprite sprite; // Level Icon.
}
