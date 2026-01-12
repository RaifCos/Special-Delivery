using UnityEngine;

public class ShopManager : MonoBehaviour {

    /*
    [Header("Shop Variables")]
    public GameObject buttonIcons;
    public GameObject itemDisplay;
    public Sprite lockedSprite;
    public Sprite[] shopSprite;
    */

    private ShopUpgrade[] upgrades;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        GameManager.shopManager = this;
    }

    // Update is called once per frame
    void Update() {
        
    }
}
