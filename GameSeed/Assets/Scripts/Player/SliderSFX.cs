using UnityEngine;
using UnityEngine.EventSystems;

public class SliderSFX : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.Instance.PlayLoopingSFX("Slider");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AudioManager.Instance.StopLoopingSFX();
    }
}