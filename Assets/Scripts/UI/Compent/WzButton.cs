using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WzButton : Button
{

    public override void OnPointerClick(PointerEventData eventData)
    {
        AudioService.Instance.ButtonSE();
        base.OnPointerClick(eventData);
    }

}
