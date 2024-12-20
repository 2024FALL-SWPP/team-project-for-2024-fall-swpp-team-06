using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayManager : MonoBehaviour
{
    public List<GameObject> pools = new List<GameObject>();
    private float gridSize;

    // TODO: change to Interaction Range
    private FarmingManager farmingManager;

    private void Start()
    {
        farmingManager = FarmingManager.Instance;
        gridSize = farmingManager.gridSize;
        
        for (int i = 0; i < pools.Count; i++)
        {
            pools[i] = Instantiate(pools[i]);
            pools[i].transform.localScale = gridSize * 0.1f * Vector3.one;
            pools[i].SetActive(false);
        }
    }

    public void SetOverlayInvisible()
    {
        pools[0].SetActive(false);
        pools[1].SetActive(false);
    }

    public void ChangeOverlay(OverlayData overlayData)
    {
        if (overlayData.canFarm)
        {
            pools[1].transform.position = overlayData.position;
            pools[1].transform.rotation = overlayData.rotation;
            pools[0].SetActive(false);
            pools[1].SetActive(true);
        }
        else
        {
            pools[0].transform.position = overlayData.position;
            pools[0].transform.rotation = overlayData.rotation;
            pools[1].SetActive(false);
            pools[0].SetActive(true);
        }
    }
    
}
