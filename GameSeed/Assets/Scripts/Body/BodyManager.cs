using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BodyManager : MonoBehaviour
{
    public static BodyManager Instance;
    public const int maxBody = 10;
    public BodyInstance[] bodyOwned = new BodyInstance[maxBody];
    public int bodyOwnedNeff = 0;
    public BodyInstance currentEquippedBody;
    public BodyInstance tempPreviewBody;
    public StickBody stickBody;
    public BodyType def;

    void Awake()
    {
        if (currentEquippedBody == null || currentEquippedBody.data == null)
        {
            currentEquippedBody = new BodyInstance(def);
        }
        if(currentEquippedBody.data == null)
        {
            Debug.LogError("null di data");
        } else
        {
            Debug.Log("aman kok");
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        
    }

    public void AddBodyToInventory(BodyInstance body)
    {
        if(bodyOwnedNeff >= maxBody) return;
        bodyOwned[bodyOwnedNeff] = body;
        bodyOwnedNeff++;
    }

    public void PreviewBody(int index)
    {
        if(index >= bodyOwnedNeff) return;
        BodyInstance body = bodyOwned[index];
        if(body == null) return;

        tempPreviewBody = body;

        stickBody.ApplyPreview(body.data);
    }

    public void ConfirmBody()
    {
        if(tempPreviewBody == null) return;
        currentEquippedBody = tempPreviewBody;
    }

    public void CancelPreviewBody()
    {
        if(currentEquippedBody == null) return;
        stickBody.ApplyPreview(currentEquippedBody.data);
        tempPreviewBody = currentEquippedBody;
    }

}
