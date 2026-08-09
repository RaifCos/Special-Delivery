using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DisplayIcon : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,   
    IDeselectHandler  
{
    [SerializeField] private int type;
    private GameObject obj;
    Color32 defaultColour = new(190, 190, 190, 255);
    Color32 selectedColour = new(255, 255, 255, 255);

    void OnEnable() {
        obj = gameObject;
        obj.GetComponent<Image>().color = defaultColour;
    }

    private void ValueEnter() {
        obj.GetComponent<Image>().color = selectedColour;
        switch (type) {
            case 0: { // Shop Upgrade
                GameManager.garageMenuManager.DisplayUpgrade(obj.name);
                break; }
            case 1: { // Obstacle Icon
                GameManager.galleryManager.DisplayObstacle(obj.name);
                break; }
            case 2: { // Prop Icon
                GameManager.galleryManager.DisplayProp(obj.name);
                break; }
            case 3: { // Achievement Icon
                GameManager.achievementMenuManager.DisplayAchievement(obj.name);
                break; }
            case 4: { // Level Icon
                GameManager.mainMenuManager.DisplayLevel(obj.name);
                break; }
        }
    }

    private void ValueExit() => obj.GetComponent<Image>().color = defaultColour;

    public void OnPointerEnter(PointerEventData eventData) => ValueEnter();
    public void OnPointerExit(PointerEventData eventData) => ValueExit();

    public void OnSelect(BaseEventData eventData) => ValueEnter();
    public void OnDeselect(BaseEventData eventData) => ValueExit();
}