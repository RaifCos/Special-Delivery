using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour {

    [Header("Achievement Menu Variables")]
    public Sprite lockedSprite;
    public GameObject obstacleIcons, propIcons;
    public GameObject displayName, displayCount, displayImage, displayDesc, switchButtonA, switchButtonB;


    void Awake() { GameManager.galleryManager = this; }

    public void AlternateGalleryMenus(bool input) {
        obstacleIcons.SetActive(input);
        propIcons.SetActive(!input);
        switchButtonA.SetActive(input);
        switchButtonB.SetActive(!input);
        if (input) { DisplayObstacle("carRed"); }
        else { DisplayProp("stopSign"); }
    }

    // Update is called once per frame
    public void UpdateGalleryUI() {
        foreach (Obstacle obs in GameManager.dataManager.GetObstacles()) {
            string name = obs.so.internalName;
            Image img = obstacleIcons.transform.Find(name).GetComponent<Image>();
            if (PlayerPrefs.GetInt("EncounterObs_" + name, 0) > 0) {
                img.sprite = obs.so.sprite;
            } else { img.sprite = lockedSprite; }
        }
        
        foreach (Prop prop in GameManager.dataManager.GetProps()) {
            string name = prop.so.internalName;
            Image img = propIcons.transform.Find(name).GetComponent<Image>();
            if (PlayerPrefs.GetInt("EncounterProp_" + name, 0) > 0) {
                img.sprite = prop.so.sprite;
            } else { img.sprite = lockedSprite; }
        }
    }

    public void DisplayObstacle(string key) {
        Image img = obstacleIcons.transform.Find(key).GetComponent<Image>();
        Obstacle obs = GameManager.dataManager.GetObstacle(key);
        displayImage.GetComponent<Image>().sprite = img.sprite;
        // Achievement is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            displayName.GetComponent<TMP_Text>().text = "???";
            displayDesc.GetComponent<TMP_Text>().text = "you haven't encountered into this obstacle yet.";
            displayCount.GetComponent<TMP_Text>().text = "Total Encountered: 0";
        } else { // Achievement is unlocked, so show achievement information.
            displayName.GetComponent<TMP_Text>().text = obs.so.externalName;
            displayDesc.GetComponent<TMP_Text>().text = obs.so.description;
            displayCount.GetComponent<TMP_Text>().text = "Total Encountered: " + PlayerPrefs.GetInt("EncounterObs_" + key, 0);
        }
    }
    
    public void DisplayProp(string key) {
        Image img = propIcons.transform.Find(key).GetComponent<Image>();
        Prop prop = GameManager.dataManager.GetProp(key);
        displayImage.GetComponent<Image>().sprite = img.sprite;
        // Achievement is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            displayName.GetComponent<TMP_Text>().text = "???";
            displayDesc.GetComponent<TMP_Text>().text = "you haven't, um.. crashed into this obstacle yet.";
            displayCount.GetComponent<TMP_Text>().text = "Total Destroyed: 0";
        } else { // Achievement is unlocked, so show achievement information.
            displayName.GetComponent<TMP_Text>().text = prop.so.externalName;
            displayDesc.GetComponent<TMP_Text>().text = prop.so.description;
            displayCount.GetComponent<TMP_Text>().text = "Total Destroyed: " + PlayerPrefs.GetInt("EncounterProp_" + key, 0);
        }
    }
}
