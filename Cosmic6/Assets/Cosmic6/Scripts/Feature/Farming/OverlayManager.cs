using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayManager : MonoBehaviour
{
    public List<GameObject> pools = new List<GameObject>();
    private float gridSize;

    public float overlayRange = 40f;

    private int currentX;
    private int currentZ;
    // should be ge 2
    private int checkNumSqrt = 3;
    private float normalThreshold = Mathf.Cos(Mathf.Deg2Rad * 15f);

    // currently detect default only
    private LayerMask collisionMask = 1 << 0;
    private LayerMask terrainMask = 1 << 8;

    private float checkRate = 0.15f;
    private bool isStart = true;
    private bool currentCanFarming = false;
    private bool isInvisible;
    
    // enable in farming mode
    void OnEnable()
    {
        if (isStart)
        {
            for (int i = 0; i < pools.Count; i++)
            {
                pools[i] = Instantiate(pools[i]);
                pools[i].SetActive(false);
            }

            isStart = false;
        }

        isInvisible = true;
        
        //StartCoroutine(TrackMousePointer());
    }
/*
    IEnumerator TrackMousePointer()
    {
        while (true)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, overlayRange, terrainMask | collisionMask))
            {
                
                if (hit.collider.CompareTag("Terrain"))
                {
                    Vector3 hitPoint = hit.point;
                    Terrain hitTerrain = hit.collider.GetComponent<Terrain>();
                
                    int x = Mathf.FloorToInt(hitPoint.x);
                    int z = Mathf.FloorToInt(hitPoint.z);
                
                    if (isInvisible || x != currentX || z != currentZ)
                    {
                        ChangeOverlay(hitPoint, hitTerrain);
                        currentX = x;
                        currentZ = z;
                        isInvisible = false;
                    }
                    
                }
                else
                {
                    isInvisible = true;
                    pools[0].SetActive(false);
                    pools[1].SetActive(false);
                }
            }
            else
            {
                isInvisible = true;
                pools[0].SetActive(false);
                pools[1].SetActive(false);
            }

            yield return new WaitForSeconds(checkRate);
        }
        
    }*/

    public void SetOverlayInvisible()
    {
        pools[0].SetActive(false);
        pools[1].SetActive(false);
    }

    public void ChangeOverlay(FarmingManager.OverlayData overlayData)
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
        
    /*
    public void ChangeOverlay(Vector3 pointGlobal, Terrain terrain)
    {
        
        int startX = Mathf.FloorToInt(pointGlobal.x);
        int startZ = Mathf.FloorToInt(pointGlobal.z);
        
        Vector3 centerNormal = Vector3.zero;
        
        float terrainSizeX = terrain.terrainData.size.x;
        float terrainSizeZ = terrain.terrainData.size.z;
            
        bool canFarming = true;

        float centerHeight = 0;

        int centerIdx = checkNumSqrt / 2;
        
        float checkInterval = gridSize / (checkNumSqrt - 1);

        for (int i = 0; i < checkNumSqrt; i++)
        {
            float x = startX + i * checkInterval;

            for (int j = 0; j < checkNumSqrt; j++)
            {
                float z = startZ + j * checkInterval;
                
                float localX = x - terrain.transform.position.x;
                float localZ = z - terrain.transform.position.z;

                float height = GetHeightGlobal(localX, localZ, terrain);

                if (i == centerIdx && j == centerIdx)
                {
                    centerHeight = height;
                }
                
                Vector3 normal = terrain.terrainData.GetInterpolatedNormal(localX / terrainSizeX, localZ / terrainSizeZ);

                if (normal.y < normalThreshold)
                {
                    canFarming = false;
                }
                
            }
        }
        
        Vector3 center = new Vector3(startX + gridSize / 2, centerHeight + 1f, startZ + gridSize / 2);


        if (Physics.Raycast(center, Vector3.down, out RaycastHit hit, 5, terrainMask))
        {
            center = hit.point;
            centerNormal = hit.normal;
        }
        
        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, centerNormal);
        
        Vector3 pos = center + 0.1f * centerNormal;
        
        if (canFarming)
        {
            float radius = gridSize / 2;

            Collider[] hits = Physics.OverlapBox(center, new Vector3(radius, radius / 2, radius), normalRotation,
                collisionMask);

            if (hits.Length > 0)
            {
                canFarming = false;
            }
        }
        
        if (canFarming)
        {
            pools[1].transform.position = pos;
            pools[1].transform.rotation = normalRotation;
            pools[0].SetActive(false);
            pools[1].SetActive(true);
        }
        else
        {
            pools[0].transform.position = pos;
            pools[0].transform.rotation = normalRotation;
            pools[1].SetActive(false);
            pools[0].SetActive(true);
        }
    }*/
    
}
