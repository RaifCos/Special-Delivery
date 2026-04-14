using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    
    public GameObject obj;
    public string message;
    TMP_Text tmp;

    void Start() { tmp = obj.GetComponent<TMP_Text>(); }

    public void OnPointerEnter(PointerEventData eventData) {
        if(message.Equals("COMPLETE 25 DELIVERIES TO UNLOCK.")) { message += " [" + GameManager.dataManager.GetLifetimeDeliveries() + "/25]"; }
        tmp.text = message;
    }

    public void OnPointerExit(PointerEventData eventData) {
        tmp.text = "";   
    }
}
