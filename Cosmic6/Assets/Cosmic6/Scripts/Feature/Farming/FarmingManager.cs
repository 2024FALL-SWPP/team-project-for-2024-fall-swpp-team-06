using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FieldState
{
    NotTilled,  // not cultivated
    Tilled,     // cultivated but not planted
    Planted
}


public class FarmingManager : MonoBehaviour
{
    public struct OverlayData
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool canFarm;

        public OverlayData(Vector3 position, Quaternion rotation, bool canFarm)
        {
            this.position = position;
            this.rotation = rotation;
            this.canFarm = canFarm;
        }
    }
    
    // fieldStates should be saved & loaded
    private Dictionary<(int, int), FieldState> fieldStates = new Dictionary<(int, int), FieldState>();
    private OverlayManager overlayManager;
    private FieldDecayManager fieldDecayManager;

    public float gridSize = 1f;
    public bool isFarmingMode = false;
    private bool isStop = true;
    
    // currently detect default only
    private LayerMask collisionMask = 1 << 0;
    private LayerMask terrainMask = 1 << 8;

    private bool isOverlayInvisible = false;
    
    private int currentX;
    private int currentZ;
    // should be ge 2
    private int checkNumSqrt = 3;
    public float angleThresholdDeg = 15f;
    private float normalThreshold;
    
    private float checkRate = 0.15f;

    private bool isClicked = false;
    
    
    
    /// <summary>
    /// Input coordinates are Floor(global position / gridSize)
    /// </summary>
    
    void Start()
    {
        // load fieldStates
        normalThreshold = Mathf.Cos(Mathf.Deg2Rad * angleThresholdDeg);
        overlayManager = GetComponent<OverlayManager>();
        fieldDecayManager = GetComponent<FieldDecayManager>();
    }

    void Update()
    {
        if (!isFarmingMode && !isStop)
        {
            StopCoroutine("FarmingRoutine");
            isStop = true;
        }

        if (isFarmingMode && isStop)
        {
            StartCoroutine("FarmingRoutine");
            isStop = false;
        }

        if (!isClicked && Input.GetMouseButtonDown(0))
        {
            isClicked = true;
        }
    }

    IEnumerator FarmingRoutine()
    {
        while (true)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit,
                    overlayManager.overlayRange, terrainMask | collisionMask))
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    Vector3 hitPoint = hit.point;
                    Terrain hitTerrain = hit.collider.GetComponent<Terrain>();
                
                    int x = Mathf.FloorToInt(hitPoint.x / gridSize);
                    int z = Mathf.FloorToInt(hitPoint.z / gridSize);

                    FieldState state = GetFieldState(x, z);

                    if (state == FieldState.NotTilled)
                    {
                        if (isOverlayInvisible || x != currentX || z != currentZ)
                        {
                            OverlayData overlayData = GetOverlayData(x, z, hitTerrain);

                            // TODO: check if equipping shovel
                            if (overlayData.canFarm && isClicked)
                            {
                                fieldDecayManager.Tile(x, z, hitTerrain);
                            }
                            
                            overlayManager.ChangeOverlay(overlayData);
                            currentX = x;
                            currentZ = z;
                            isOverlayInvisible = false;
                        }
                    }
                    else
                    {
                        isOverlayInvisible = true;
                        overlayManager.SetOverlayInvisible();

                        if (state == FieldState.Tilled)
                        {
                            // Player Inventory -> If Equip Plant -> Plant
                        }
                        else if (state == FieldState.Planted)
                        {
                            // if clicked -> Harvest
                        }
                    }
                }
                else
                {
                    isOverlayInvisible = true;
                    overlayManager.SetOverlayInvisible();
                }
            }
            else
            {
                isOverlayInvisible = true;
                overlayManager.SetOverlayInvisible();
            }

            yield return new WaitForSeconds(checkRate);
        }
    }

    OverlayData GetOverlayData(int x, int z, Terrain terrain)
    {
        float terrainSizeX = terrain.terrainData.size.x;
        float terrainSizeZ = terrain.terrainData.size.z;
        
        Vector3 centerNormal = Vector3.zero;
            
        bool canFarming = true;

        float centerHeight = 0;

        int centerIdx = checkNumSqrt / 2;
        
        float checkInterval = gridSize / (checkNumSqrt - 1);

        for (int i = 0; i < checkNumSqrt; i++)
        {
            float curX = x + i * checkInterval;

            for (int j = 0; j < checkNumSqrt; j++)
            {
                float curZ = z + j * checkInterval;
                
                float localX = curX - terrain.transform.position.x;
                float localZ = curZ - terrain.transform.position.z;

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
        
        Vector3 center = new Vector3(x + gridSize / 2, centerHeight + 1f, z + gridSize / 2);
        
        if (Physics.Raycast(center, Vector3.down, out RaycastHit hit, 2, terrainMask))
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
        
        return new OverlayData(pos, normalRotation, canFarming);
    }
    
    
    
    float GetHeightGlobal(float localX, float localZ, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        
        int heightmapWidth = terrainData.heightmapResolution;
        int heightmapHeight = terrainData.heightmapResolution;
        int xIndex = Mathf.FloorToInt(Mathf.Clamp((localX / terrainData.size.x) * heightmapWidth, 0.1f,
            heightmapWidth - 0.9f));
        int zIndex = Mathf.FloorToInt(Mathf.Clamp((localZ / terrainData.size.z) * heightmapHeight, 0.1f,
            heightmapHeight - 0.9f));

        return terrain.transform.position.y + terrainData.GetHeight(xIndex, zIndex);
    }
    
    public FieldState GetFieldState(int x, int z)
    {
        if (fieldStates.TryGetValue((x, z), out FieldState state))
        {
            return state;
        }
        
        return FieldState.NotTilled;
    }
    
    public bool SetFieldState(int x, int z, bool isPlanted)
    {
        if (!fieldStates.ContainsKey((x, z)))
        {
            return false;
        }

        fieldStates[(x, z)] = isPlanted ? FieldState.Planted : FieldState.Tilled;
        return true;
    }

    public bool RemoveFieldState(int x, int z)
    {
        return fieldStates.Remove((x, z));
    }
    
    public bool AddTilledField(int x, int z)
    {
        if (!fieldStates.ContainsKey((x, z)))
        {
            fieldStates[(x, z)] = FieldState.Tilled;
            return true;
        }

        return false;
    }
    
    public List<(int, int)> GetTilledFields()
    {
        List<(int, int)> tilledFields = new List<(int, int)>();

        foreach (var kvp in fieldStates)
        {
            if (kvp.Value == FieldState.Tilled)
            {
                tilledFields.Add(kvp.Key);
            }
        }

        return tilledFields;
    }
    
}
