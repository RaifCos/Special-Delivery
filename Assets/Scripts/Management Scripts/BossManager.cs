using UnityEngine;

// Script to handle objectives (Parcels and Delivery Spots)
public class BossManager : MonoBehaviour {
    void Awake() { GameManager.bossManager = this; }
}