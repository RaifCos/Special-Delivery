using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuText : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,   
    IDeselectHandler    
{
    public GameObject obj;
    public string message;
    TMP_Text tmp;

    void OnEnable() { tmp = obj.GetComponent<TMP_Text>(); }

    private void ValueEnter() {
        if (message.Equals("COMPLETE 25 DELIVERIES TO UNLOCK")) { message += " [" + GameManager.dataManager.GetLifetimeDeliveries() + "/25]"; }
        tmp.text = message;
    }

    private void ValueExit() => tmp.text = "";

    public void OnPointerEnter(PointerEventData eventData) => ValueEnter();
    public void OnPointerExit(PointerEventData eventData) => ValueExit();

    public void OnSelect(BaseEventData eventData) => ValueEnter();
    public void OnDeselect(BaseEventData eventData) => ValueExit();
}