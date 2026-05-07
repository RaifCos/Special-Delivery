using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level_SO", menuName = "Scriptable Objects/Level")]
public class Level_SO : ScriptableObject {
    public string internalName, externalName;

    [Header("Music Audio Clips")] 
    public AudioClip musicStart;
    public AudioClip musicLoop;
    public AudioClip musicEnd;

    [Header("Object Pools")] 
    public List<Obstacle> startingObstacles;
    public List<Obstacle> permObstacles;
    public List<Obstacle> tempObstacles;

    [Header("Other Variables")]
    public List<Level_SO> requirements; 
}
