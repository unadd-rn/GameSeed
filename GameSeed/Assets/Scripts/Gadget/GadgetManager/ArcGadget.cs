using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArcGadget : MonoBehaviour
{
    public RectTransform rectTransform => GetComponent<RectTransform>();

    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private GameObject activeIndicator;
    [SerializeField] private CanvasGroup canvasGroup;

    public GadgetInstance Instance { get; private set; }

    public void Init(GadgetInstance instance)
    {
        Instance = instance;
        icon.sprite = instance.data.model;
        nameLabel.text = instance.data.gadgetName;
    }

    public void SetActiveSlot(bool active)
    {
        activeIndicator?.SetActive(active);
        if(canvasGroup) canvasGroup.alpha = active ? 1f : 0.6f;
    }

}
