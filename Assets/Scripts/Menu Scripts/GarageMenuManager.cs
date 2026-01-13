using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GarageMenuManager : MonoBehaviour {

    [Header("Garage Menu Variables")]
    public GameObject buttonIcons, cashCounter, upgradeDisplay;
    public Sprite lockedSprite;
    private int cash;
    private string listed;

    void Awake() { GameManager.garageMenuManager = this; }

    void Start() { 
        UpdateCash();
        foreach(Upgrade_SO up in GameManager.dataManager.GetUpgrades()) {
            UpdateUpgradeUI(up.internalName);
        }  
    }

    public string GetListed() => listed;

    private void UpdateCash() {
        cash = GameManager.dataManager.GetCash();
        cashCounter.GetComponent<TMP_Text>().text = string.Format("{0:#,##0.##}", cash);
    }

    private void UpdateUpgradeUI(string key) {
        Image img = buttonIcons.transform.Find(key).GetComponent<Image>();
        if (GameManager.dataManager.IsUnlocked(key)) { 
            img.sprite = GameManager.dataManager.GetAchievement(key).sprite;
            if (GameManager.dataManager.IsUpgraded(key)) { img.color = new Color32(255, 223, 43, 255); }
            else { img.color = Color.white; }
        } else { img.sprite = lockedSprite; }
    }

    public void DisplayUpgrade(string key) {
        Image img = buttonIcons.transform.Find(key).GetComponent<Image>();
        upgradeDisplay.transform.GetChild(0).GetComponent<Image>().sprite = img.sprite;
        Upgrade_SO up = GameManager.dataManager.GetUpgrade(key);
        // Upgrade is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            upgradeDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = "???";
            upgradeDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = "you'll need to by some other upgrades first.";
            upgradeDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = "???";
        } else { // Upgrade is unlocked, so show information.
            upgradeDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = up.externalName;
            upgradeDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = up.description;
            upgradeDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = string.Format("{0:#,##0.##}", up.cost);
        } listed = key;
    }  
}
