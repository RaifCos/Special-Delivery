using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GalleryManager : MonoBehaviour {

    [Header("Achievement Menu Variables")]
    public Sprite lockedSprite;
    public GameObject obstacleIcons, propIcons;
    public GameObject galleryDisplay, switchButtonA, switchButtonB;
    private List<Obstacle> obstacles;
    private List<Prop> props;

    void Awake() { GameManager.galleryManager = this; }

    void Start() { 
        obstacles = GameManager.obstacleData.GetObstacles(); 
        props = GameManager.obstacleData.GetProps();    
    }

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
        foreach (Obstacle obs in obstacles) {
            string name = obs.so.internalName;
            Debug.Log(name);
            Image img = obstacleIcons.transform.Find(name).GetComponent<Image>();
            if (PlayerPrefs.GetInt("EncounterObs_" + name, 0) > 0) {
                img.sprite = obs.so.sprite;
            } else { img.sprite = lockedSprite; }
        }
        
        foreach (Prop prop in props) {
            string name = prop.so.internalName;
            Image img = propIcons.transform.Find(name).GetComponent<Image>();
            if (PlayerPrefs.GetInt("EncounterProp_" + name, 0) > 0) {
                img.sprite = prop.so.sprite;
            } else { img.sprite = lockedSprite; }
        }
    }

    public void DisplayObstacle(string key) {
        Image img = obstacleIcons.transform.Find(key).GetComponent<Image>();
        Obstacle obs = GameManager.obstacleData.GetObstacle(key);
        galleryDisplay.transform.GetChild(0).GetComponent<Image>().sprite = img.sprite;
        galleryDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = "Total Encountered: " + PlayerPrefs.GetInt("EncounterObs_" + key, 0);;
        // Achievement is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            galleryDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = "???";
            galleryDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = "you haven't encountered into this obstacle yet.";
        } else { // Achievement is unlocked, so show achievement information.
            galleryDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = obs.so.externalName;
            galleryDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = obs.so.description;
        }
    }
    
    public void DisplayProp(string key) {
        Image img = propIcons.transform.Find(key).GetComponent<Image>();
        Prop prop = GameManager.obstacleData.GetProp(key);
        galleryDisplay.transform.GetChild(0).GetComponent<Image>().sprite = img.sprite;
        galleryDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = "Total Destroyed: " + PlayerPrefs.GetInt("EncounterProp_" + key, 0);;
        // Achievement is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            galleryDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = "???";
            galleryDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = "you haven't, um.. crashed into this obstacle yet.";
        } else { // Achievement is unlocked, so show achievement information.
            galleryDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = prop.so.externalName;
            galleryDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = prop.so.description;
        }
    }
}
