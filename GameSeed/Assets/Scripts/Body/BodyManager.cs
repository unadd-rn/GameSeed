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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("BodyManager");
        BodyManager mgr = go.AddComponent<BodyManager>();
        Instance = mgr;
        DontDestroyOnLoad(go);

        mgr.def = Resources.Load<BodyManagerSettings>("BodyManagerSettings").defaultBody; // move the asset into Resources
        mgr.LoadBodyData();

        for(int i = 0; i < mgr.bodyOwned.Length; i++)
            if(mgr.bodyOwned[i] != null)
                mgr.bodyOwnedNeff++;

        mgr.currentEquippedBody = null;
        if(mgr.currentEquippedBody == null || mgr.currentEquippedBody.data == null)
        {
            bool found = false;
            for (int i = 0; i < mgr.bodyOwnedNeff; i++)
            {
                if (mgr.bodyOwned[i].isEquipped)
                {
                    mgr.currentEquippedBody = mgr.bodyOwned[i];
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                mgr.currentEquippedBody = new BodyInstance(mgr.def);
                mgr.AddBodyToInventory(mgr.currentEquippedBody);
            }
        }
    }


    // void Awake()
    // {
    //     if (currentEquippedBody == null || currentEquippedBody.data == null)
    //     {
    //         currentEquippedBody = new BodyInstance(def);
    //         AddBodyToInventory(currentEquippedBody);
    //     }
    //     if(currentEquippedBody.data == null)
    //     {
    //         Debug.LogError("null di data");
    //     } else
    //     {
    //         Debug.Log("aman kok");
    //     }

    //     if (Instance != null && Instance != this)
    //     {
    //         Destroy(gameObject);
    //         return;
    //     }

    //     Instance = this;

    //     DontDestroyOnLoad(gameObject);
    // }

    void Start()
    {
        
    }

    public void AddBodyTypeToInventory(BodyType bodyTypeData)
    {
        if (bodyTypeData == null) return;
        BodyInstance newInstance = new BodyInstance(bodyTypeData);
        AddBodyToInventory(newInstance);
        Debug.Log($"Drop Instance {bodyTypeData.stickName} ke dalam inventory player!");
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
        if(currentEquippedBody != null)
            currentEquippedBody.isEquipped = false;

        currentEquippedBody = tempPreviewBody;
        currentEquippedBody.isEquipped = true;
        SaveBodyData();
    }

    public void CancelPreviewBody()
    {
        if(currentEquippedBody == null) return;
        stickBody.ApplyPreview(currentEquippedBody.data);
        tempPreviewBody = currentEquippedBody;
    }

    public void SaveBodyData()
    {
        string json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("BodyInventory", json);
        PlayerPrefs.Save();
    }

    public void LoadBodyData()
    {
        if (PlayerPrefs.HasKey("BodyInventory"))
        {
            string json = PlayerPrefs.GetString("BodyInventory");
            JsonUtility.FromJsonOverwrite(json, this);
            RelinkData();
        }
    }

    public void RelinkData()
    {
        for (int i = 0; i < bodyOwnedNeff; i++)
        {
            if (bodyOwned[i] != null && !string.IsNullOrEmpty(bodyOwned[i].bodyTypeName))
            {
                bodyOwned[i].data = Resources.Load<BodyType>("Bodies/" + bodyOwned[i].bodyTypeName);
            }
        }
    }
}
