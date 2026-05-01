using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GarageMenuManager : MonoBehaviour {

    [Header("Garage Menu Variables")]
    [SerializeField] private GameObject buttonIcons;
    [SerializeField] private GameObject cashCounter;
    [SerializeField] private GameObject upgradeDisplay;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private AudioClip buySound;
    private int cash;
    private string listed;
    private Color32 gold = new(255, 223, 43, 255);

    void Awake() { GameManager.garageMenuManager = this; }

    void Start() { UpdateCash(); }

    public string GetListed() => listed;

    private void UpdateCash() {
        cash = GameManager.dataManager.GetCash();
        cashCounter.GetComponent<TMP_Text>().text =string.Format("{0:#,##0.##}", cash);
    }

    public void UpdateMenu(bool purchase) {
        UpdateCash();
        foreach (Upgrade_SO up in GameManager.dataManager.GetUpgrades()) {
            UpdateUpgradeUI(up.internalName);
        } 
        
        if (purchase) { 
            GameManager.audioManager.PlaySoundEffect(buySound);
            DisplayUpgrade(listed);
        } 
    }

    private void UpdateUpgradeUI(string key) {
        Image img = buttonIcons.transform.Find(key).GetComponent<Image>();
        if (GameManager.dataManager.IsUnlocked(key)) { 
            img.sprite = GameManager.dataManager.GetUpgrade(key).sprite;
        } else { img.sprite = lockedSprite; }
    }

    public void DisplayUpgrade(string key) {
        Image img = buttonIcons.transform.Find(key).GetComponent<Image>();
        Image displayImg = upgradeDisplay.transform.GetChild(0).GetComponent<Image>();

        // Display Image in Corner (Golden if already purchased.)
        displayImg.sprite = img.sprite;
        displayImg.color = Color.white;

        Upgrade_SO up = GameManager.dataManager.GetUpgrade(key);

        // Upgrade is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            upgradeDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = "???";
            upgradeDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = "you'll need to buy some other upgrades first.";
            upgradeDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = "???";
        } else { // Upgrade is unlocked, so show information.
            upgradeDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = up.externalName;
            upgradeDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = up.description;
            if (GameManager.dataManager.IsUpgraded(key)) { 
                displayImg.color = gold;
                upgradeDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = "Purchased";
            } else {
                upgradeDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = string.Format("{0:#,##0.##}", up.cost);
            }
        } listed = key;
    } 
}
