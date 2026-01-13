using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public class GarageMenuManager : MonoBehaviour {

    [Header("Garage Menu Variables")]
    public GameObject buttonIcons, cashCounter, upgradeDisplay;
    public Button buyButton;
    public Sprite lockedSprite;
    private int cash;
    private string listed;

    void Awake() { GameManager.garageMenuManager = this; }

    void Start() { UpdateCash(); }

    public string GetListed() => listed;

    private void UpdateCash() {
        cash = GameManager.dataManager.GetCash();
    }

    public void UpdateMenu() {
        cashCounter.GetComponent<TMP_Text>().text = string.Format("{0:#,##0.##}", cash);
        foreach (Upgrade_SO up in GameManager.dataManager.GetUpgrades()) {
            UpdateUpgradeUI(up.internalName);
        }
    }

    private void UpdateUpgradeUI(string key) {
        Debug.Log(key);
        Image img = buttonIcons.transform.Find(key).GetComponent<Image>();
        if (GameManager.dataManager.IsUnlocked(key)) { 
            img.sprite = GameManager.dataManager.GetUpgrade(key).sprite;
            if (GameManager.dataManager.IsUpgraded(key)) { img.color = new Color32(255, 223, 43, 255); }
            else { img.color = Color.white; }
        } else { img.sprite = lockedSprite; }
    }

    private void ToggleBuyButton(bool isOn) {
        buyButton.interactable = isOn;
        if (isOn) { buyButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "BUY"; }
        else { buyButton.transform.GetChild(0).GetComponent<TMP_Text>().text = ""; }
    }

    public void DisplayUpgrade(string key) {
        Image img = buttonIcons.transform.Find(key).GetComponent<Image>();
        Image displayImg = upgradeDisplay.transform.GetChild(0).GetComponent<Image>();
        displayImg.sprite = img.sprite;

        Upgrade_SO up = GameManager.dataManager.GetUpgrade(key);

        // Upgrade is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            upgradeDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = "???";
            upgradeDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = "you'll need to buy some other upgrades first.";
            upgradeDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = "???";
            ToggleBuyButton(false);
        } else { // Upgrade is unlocked, so show information.
            Debug.Log(up.externalName);
            Debug.Log(upgradeDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text);
            upgradeDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = up.externalName;
            upgradeDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = up.description;
            upgradeDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = string.Format("{0:#,##0.##}", up.cost);
            ToggleBuyButton(true);
        } listed = key;

        ToggleBuyButton(!GameManager.dataManager.IsUpgraded(key) && GameManager.dataManager.IsUnlocked(key));
    } 
}
