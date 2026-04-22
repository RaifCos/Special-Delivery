using UnityEngine;
using UnityEngine.EventSystems;

public class DisplayIcon : MonoBehaviour,
    IPointerEnterHandler,
    ISelectHandler  
{
    private GameObject obj;
    public int type;

    void OnEnable() { obj = gameObject; }

    private void ValueEnter() {
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
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => ValueEnter();

    public void OnSelect(BaseEventData eventData) => ValueEnter();
}