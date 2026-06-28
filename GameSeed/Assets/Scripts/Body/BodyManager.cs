using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BodyManager : MonoBehaviour
{
    public BodyInstance[] bodyOwned = new BodyInstance[10];
    public BodyInstance currentEquippedBody;
    public BodyInstance tempPreviewBody;
    
    public StickBody stickBody;

    void Start()
    {
        if(currentEquippedBody != null)
        {
            stickBody.ApplyPreview(currentEquippedBody.data);
            tempPreviewBody = currentEquippedBody;
        }
    }

    public void PreviewBody(int index)
    {
        if(index >= bodyOwned.Length) return;
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
