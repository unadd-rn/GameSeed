using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable] 
public class TutorialElement
{
    public string elementName;
    public RectTransform rectTransform;
}

public class TutorialManager : MonoBehaviour
{
    [Header("Overlay")]
    [SerializeField] private Image overlayImage;
    [SerializeField] private Material overlayMaterial;

    [Header("Settings")]
    [SerializeField] private float padding = 0.02f;       // extra space disekitar elemennya loh
    [SerializeField] private float cornerRadius = 0.01f;
    [SerializeField] private float softness = 0.005f;

    [Header("Elements")]
    [SerializeField] private List<TutorialElement> elements;

    private static readonly int CenterX = Shader.PropertyToID("_CenterX");
    private static readonly int CenterY = Shader.PropertyToID("_CenterY");
    private static readonly int Width = Shader.PropertyToID("_Width");
    private static readonly int Height = Shader.PropertyToID("_Height");
    private static readonly int CornerRadius = Shader.PropertyToID("_CornerRadius");
    private static readonly int Softness = Shader.PropertyToID("_Softness");

    private void Start()
    {
        overlayImage.material = overlayMaterial;
        HideTutorial();
    }

    public void ShowTutorial(string elementName)
    {
        TutorialElement target = elements.Find(e => e.elementName == elementName);

        if (target == null)
        {
            ShowFullDim();
            return;
        }
        overlayImage.gameObject.SetActive(true);

        Vector3[] corners = new Vector3[4];
        target.rectTransform.GetWorldCorners(corners);

        float minX = corners[0].x / Screen.width;
        float maxX = corners[2].x / Screen.width;
        float minY = corners[0].y / Screen.height;
        float maxY = corners[2].y / Screen.height;

        float centerX = (minX + maxX) * 0.5f;
        float centerY = (minY + maxY) * 0.5f;
        float width = (maxX - minX) + padding;
        float height = (maxY - minY) + padding;

        overlayMaterial.SetFloat(CenterX, centerX);
        overlayMaterial.SetFloat(CenterY, centerY);
        overlayMaterial.SetFloat(Width, width);
        overlayMaterial.SetFloat(Height, height);
        overlayMaterial.SetFloat(CornerRadius, cornerRadius);
        overlayMaterial.SetFloat(Softness, softness);
    }

    public void ShowFullDim()
    {
        overlayImage.gameObject.SetActive(true);
        overlayMaterial.SetFloat(Width, 0);
        overlayMaterial.SetFloat(Height, 0);
    }

    public void HideTutorial()
    {
        overlayImage.gameObject.SetActive(false);
    }
}
