using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class CameraRaycaster : MonoBehaviour
{
    private float raycastPeriod = 0.1f;
    private float raycastDistance = 15f;
    public UIManager uiManager;
    
    //public FarmingManager farmingManager;
    public bool isClicked { get; private set; } = false;
    public RaycastHit raycastHit { get; private set; }
    
    public const float interactionDistance = 20f;
    
    // isValid, hit, isClicked
    public event Action<bool, RaycastHit, bool> OnRaycastHit;
    
    // default & clickable & terrain: extend if needed
    private LayerMask raycastMask = (1 << 0) | (1 << 3) | (1 << 8);
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RaycastRoutine());
    }

    IEnumerator RaycastRoutine()
    {
        while (true)
        {
            if (uiManager.isUIActive)
            {
                yield return new WaitForSeconds(raycastPeriod);
                isClicked = false;
                continue;
            }
            
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit,
                    raycastDistance, raycastMask))
            {
                OnRaycastHit?.Invoke(true, hit, isClicked);
            }
            else
            {
                RaycastHit emptyHit = default;
                OnRaycastHit?.Invoke(false, emptyHit, isClicked);
            }
            
            isClicked = false;
            yield return new WaitForSeconds(raycastPeriod);
        }
    }
    
    

    // Update is called once per frame
    void Update()
    {
        if (!isClicked && Input.GetMouseButtonDown(0))
        {
            isClicked = true;
        }
    }
}
