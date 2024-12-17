using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    // Start is called before the first frame update

    public int regionIndex;
    public LocationTracker locationTracker;
    public TimeManager timeManager;
    //public Transform mainCameraTransform;
    
    private bool isActive;
    private GameObject[] monsterTypes;
    
    
    public bool isNocturnal = false;
    
    void Start()
    {
        var childCount = transform.childCount;
        
        monsterTypes = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            monsterTypes[i] = transform.GetChild(i).gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
        {
            if (locationTracker.currentRegionIndex == regionIndex)
            {
                ToggleMonsters(true);
                isActive = true;
            }
            else if (isNocturnal && timeManager.isNight)
            {
                ToggleMonsters(true);
                isActive = true;
            }
        }
        else
        {
            if (locationTracker.currentRegionIndex != regionIndex)
            {
                ToggleMonsters(false);
                isActive = false;
            }
            else if (isNocturnal && !timeManager.isNight)
            {
                ToggleMonsters(false);
                isActive = false;
            }
        }
    }

    void ToggleMonsters(bool activate)
    {
        
    }
}
