using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour {

    [Header("Achievement Menu Variables")]
    public Sprite lockedSprite;
    public GameObject obstacleIcons, propIcons;
    public GameObject galleryDisplay, switchButtonA, switchButtonB;
    public Sprite[] objectSprite, propSprite;
    private Obstacle[] obstacles;

    public string[,] propString = {
        {"stop sign", "a common red signage dotted around parcel city, although most drivers use them as more of a \"suggestion\" to slow down then a literal sign to stop."},
        {"street sign", "iconic green signs used to navigate parcel city. just don't touch them though, the city couldn't afford to secure them to the ground, so they're just balancing on gravity, hopes, and dreams."},
        {"traffic Cone", "if they didn't want you to run over these small orange cones, then why did they make them so fun to run over?"},
        {"bin", "what do you call it when a delivery van hits a bin? junk mail! get it? be- because bins... trash... junk... heh"},
        {"fire hydrant", "wait, shouldn't there be water coming out of these when you knock them over? guys? are these hydrants empty?? hello?? should we say something???"},
        {"bench", "sometimes it's nice to take a break from the city life and just have a quiet sit down- until a speeding delivery van knocks you four cities over."},
        {"fence", "mind you these fences are solid iron, how are you driving full force into these things without so much as a dent? are you cheating or something?"},
        {"barrier", "hey so, when you see these guys it means you're not supposed to drive here, but hey, who am i to stop you? go nuts, smash 'em all."}
    };

    void Awake() { GameManager.galleryManager = this; }

    void Start() { obstacles = GameManager.obstacleData.GetObstacles(); }

    public void AlternateGalleryMenus(bool input) {
        obstacleIcons.SetActive(input);
        propIcons.SetActive(!input);
        switchButtonA.SetActive(input);
        switchButtonB.SetActive(!input);
        if (input) { DisplayObstalce(0); }
        else { DisplayProp(0); }
    }

    // Update is called once per frame
    public void UpdateGalleryUI() {
        for (int i = 0; i < obstacles.Length; i++) {
            Image img = obstacleIcons.transform.GetChild(i).GetComponent<Image>();
            if (PlayerPrefs.GetInt("EncounterO" + i, 0) > 0) {
                img.sprite = objectSprite[i];
            } else { img.sprite = lockedSprite; }
        }
        
        for (int i = 0; i < propString.GetLength(0); i++) {
            Image img = propIcons.transform.GetChild(i).GetComponent<Image>();
            if (PlayerPrefs.GetInt("EncounterP" + i, 0) > 0) {
                img.sprite = propSprite[i];
            } else { img.sprite = lockedSprite; }
        }
    }
    
    public void DisplayObstalce(int id) {
        Image img = obstacleIcons.transform.GetChild(id).GetComponent<Image>();
        galleryDisplay.transform.GetChild(0).GetComponent<Image>().sprite = img.sprite;
        galleryDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = "Total Encountered: " + PlayerPrefs.GetInt("EncounterO" + id, 0);
        // Achievement is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            galleryDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = "???";
            galleryDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = "you haven't encountered this obstacle yet.";
        } else { // Achievement is unlocked, so show achievement information.
            galleryDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = obstacles[id].externalName;
            galleryDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = obstacles[id].description;
        } 
    }
    
    public void DisplayProp(int id) {
        Image img = propIcons.transform.GetChild(id).GetComponent<Image>();
        galleryDisplay.transform.GetChild(0).GetComponent<Image>().sprite = img.sprite;
        galleryDisplay.transform.GetChild(2).GetComponent<TMP_Text>().text = "Total Destroyed: " + PlayerPrefs.GetInt("EncounterP" + id, 0);;
        // Achievement is still locked, so show default information.
        if (img.sprite == lockedSprite) {
            galleryDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = "???";
            galleryDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = "you haven't, um.. crashed into this obstacle yet.";
        } else { // Achievement is unlocked, so show achievement information.
            galleryDisplay.transform.GetChild(1).GetComponent<TMP_Text>().text = propString[id, 0];
            galleryDisplay.transform.GetChild(3).GetComponent<TMP_Text>().text = propString[id, 1];
        }
    }
}
