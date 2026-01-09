using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    
    public GameObject obj;
    public string message;
    TMP_Text tmp;

    void Start() { tmp = obj.GetComponent<TMP_Text>(); }

    public void OnPointerEnter(PointerEventData eventData) {
        tmp.text = message;
    }

    public void OnPointerExit(PointerEventData eventData) {
        tmp.text = "";   
    }
}
