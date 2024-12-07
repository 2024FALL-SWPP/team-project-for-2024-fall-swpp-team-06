using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    public bool[] isBaseRegistered { get; private set; } = { false, false, false };
    public GameObject[] safeZoneOverlays;
    string[] validTags = { "Base1", "Base2", "Base3" };

    private int currentBase = 0;
    
    // clickable
    private int baseLayerIndex = 3;
    
    public CameraRaycaster cameraRaycaster;
    public LocationTracker locationTracker;
    public FlagManager flagManager;
    
    public QuestSystem questSystem;

    // Start is called before the first frame update
    void Start()
    {
        cameraRaycaster.OnRaycastHit += ProcessRaycast;

        for (int i = 0; i < 3; i++)
        {
            if (isBaseRegistered[i])
            {
                safeZoneOverlays[i].SetActive(true);
            }
            else
            {
                safeZoneOverlays[i].SetActive(false);
            }
        }
    }

    public void ProcessRaycast(bool isHit, RaycastHit hit, bool isClicked)
    {
        if (!isHit)
        {
            //safeZoneOverlays[currentBase].SetActive(false);
            return;
        }

        if (hit.collider.gameObject.layer == baseLayerIndex && 
                System.Array.Exists(validTags, tag => tag == hit.collider.gameObject.tag))
        {
            
            if (locationTracker.currentRegionIndex != currentBase)
            {
                //safeZoneOverlays[currentBase].SetActive(false);
                currentBase = locationTracker.currentRegionIndex;
            }
            print(currentBase);
            /*
            if (isBaseRegistered[currentBase])
            {
                safeZoneOverlays[currentBase].SetActive(true);
            }*/

            if (isClicked && !isBaseRegistered[currentBase])
            {
                isBaseRegistered[currentBase] = true;
                print("Base" + currentBase + "registered");
                safeZoneOverlays[currentBase].SetActive(true);
                flagManager.UpdateMinimap();

                if (questSystem != null)
                {
                    string variableName = validTags[currentBase];
                    questSystem.UpdateQuest(variableName, 1);
                    Debug.Log($"Quest updated for {variableName}");
                }

            }
            
        }
        /*
        else
        {
            safeZoneOverlays[currentBase].SetActive(false);
        }*/
    }
}
