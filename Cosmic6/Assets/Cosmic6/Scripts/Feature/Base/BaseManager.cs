using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using UnityEngine.Serialization;

public class BaseManager : MonoBehaviour
{
    public bool[] isBaseRegistered { get; private set; } = { false, false, false };
    public TeleUI teleUI;
    public GameObject[] mapComponents;
    public GameObject[] safeZoneOverlays;
    public GameObject[] bases;
    
    string[] validTags = { "Base1", "Base2", "Base3" };

    public Action<int> Tutorial2Complete;

    private int currentBase = 0;
    
    private GameManager gameManager;
    private bool isStart;
    
    // clickable
    private int baseLayerIndex = 3;
    
    public Action OnBaseRegistered;
    
    public CameraRaycaster cameraRaycaster;
    public LocationTracker locationTracker;
    public FlagManager flagManager;
    public MinimapController minimapController;
    
    public QuestSystem questSystem;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetComponent<GameManager>();
        isStart = gameManager.IsGameStart;
        cameraRaycaster.OnRaycastHit += ProcessRaycast;

        for (int i = 0; i < 3; i++)
        {
            if (isBaseRegistered[i])
            {
                mapComponents[i].SetActive(true);
                safeZoneOverlays[i].SetActive(true);
                bases[i].layer = 0;
            }
            else
            {
                mapComponents[i].SetActive(false);
                safeZoneOverlays[i].SetActive(false);
                bases[i].layer = baseLayerIndex;
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
            hit.collider.gameObject.tag.StartsWith("Base"))
        {
            if (locationTracker.currentRegionIndex != currentBase)
            {
                currentBase = locationTracker.currentRegionIndex;
            }
            
            print(currentBase);
            
            if (isClicked && !isBaseRegistered[currentBase])
            {
                if (isStart)
                {
                    Tutorial2Complete?.Invoke(1);
                    isStart = false;
                }
                
                isBaseRegistered[currentBase] = true;
                OnBaseRegistered?.Invoke();
                bases[currentBase].layer = 0;
                print("Base" + currentBase + "registered");
                safeZoneOverlays[currentBase].SetActive(true);
                StartCoroutine(UpdateBaseCoroutine(currentBase));

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

    IEnumerator UpdateBaseCoroutine(int baseIndex)
    {
        yield return new WaitForSeconds(8.5f);
        
        mapComponents[baseIndex].SetActive(true);
        flagManager.UpdateMinimap();
        minimapController.UpdateMinimap();
        teleUI.UpdateTeleProgress();
    }
}
