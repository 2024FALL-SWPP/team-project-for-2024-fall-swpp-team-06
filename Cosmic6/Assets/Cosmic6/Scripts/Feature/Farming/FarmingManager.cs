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
    public CameraRaycaster cameraRaycaster;

    public float gridSize { get; private set; } = 1000 / 1024f;
    public bool isFarmingMode = false;
    private bool isStop = true;

    private const float xOffset = -4000f;
    private const float zOffset = -5000f;
    
    // currently detect default only
    private LayerMask collisionMask = 1 << 0;
    private LayerMask terrainMask = 1 << 8;

    private int terrainLayerIndex = 8;

    private bool isOverlayInvisible = false;
    
    private int currentX;
    private int currentZ;
    // should be ge 2
    private int checkNumSqrt = 3;
    public float angleThresholdDeg = 15f;
    private float normalThreshold;
    
    private float checkRate = 0.15f;

    private bool isClicked = false;
    private (int, int) clickedIndices;

    // equip seed
    public bool isPlantingMode = true;
    
    // equip shovel
    public bool isTilingMode = true;
    
    
    
    /// <summary>
    /// Input coordinates are Floor(global position / gridSize)
    /// </summary>
    
    void Start()
    {
        // load fieldStates
        normalThreshold = Mathf.Cos(Mathf.Deg2Rad * angleThresholdDeg);
        overlayManager = GetComponent<OverlayManager>();
        fieldDecayManager = GetComponent<FieldDecayManager>();
        cameraRaycaster.OnRaycastHit += ProcessRaycast;
    }
    /*
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
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit,
                    overlayManager.overlayRange, terrainMask | collisionMask))
            {
                if (hit.collider.tag.StartsWith("Terrain"))
                {
                    var (x, z) = GlobalToIdx(hit.point.x, hit.point.z);
                    if (GetFieldState(x, z) == FieldState.NotTilled)
                    {
                        isClicked = true;
                        clickedIndices = (x, z);
                    }
                        
                }
            }
        }
    }

    IEnumerator FarmingRoutine()
    {
        while (true)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit,
                    overlayManager.overlayRange, terrainMask | collisionMask))
            {
                if (hit.collider.tag.StartsWith("Terrain"))
                {
                    Vector3 hitPoint = hit.point;
                    Terrain hitTerrain = hit.collider.GetComponent<Terrain>();
                    
                    var (x, z) = GlobalToIdx(hitPoint.x, hitPoint.z);

                    FieldState state = GetFieldState(x, z);

                    if (state == FieldState.NotTilled)
                    {
                        OverlayData overlayData = GetOverlayData(x, z, hitTerrain);
                        
                        if (isOverlayInvisible || x != currentX || z != currentZ)
                        {
                            overlayManager.ChangeOverlay(overlayData);
                            currentX = x;
                            currentZ = z;
                            isOverlayInvisible = false;
                        }
                        
                        // TODO: check if equipping shovel
                        if (overlayData.canFarm && isClicked)
                        {
                            if (GetFieldState(clickedIndices.Item1, clickedIndices.Item2) == FieldState.NotTilled)
                            {
                                if (fieldDecayManager.Tile(x, z, hitTerrain))
                                {
                                    print("tiling");
                                    AddTilledField(x, z);
                                }
                            }
                            isClicked = false;
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
    }*/

    public void Harvest(float globalX, float globalZ)
    {
        var (xIndex, zIndex) = GlobalToIdx(globalX, globalZ);
        
        
        
    }

    public void ProcessRaycast(bool isHit, RaycastHit hit, bool isClicked)
    {
        if (!isHit || (!isTilingMode && !isPlantingMode))
        {
            if (!isOverlayInvisible)
            {
                isOverlayInvisible = true;
                overlayManager.SetOverlayInvisible();
            }
            
            return;
        }
        
        if (hit.collider.gameObject.layer == terrainLayerIndex)
        {
            print("Terrain hit");
            Vector3 hitPoint = hit.point;
            Terrain hitTerrain = hit.collider.GetComponent<Terrain>();
        
            var (x, z) = GlobalToIdx(hitPoint.x, hitPoint.z);

            FieldState state = GetFieldState(x, z);

            if (isTilingMode && state == FieldState.NotTilled)
            {
                OverlayData overlayData = GetOverlayData(x, z, hitTerrain);
            
                if (isOverlayInvisible || x != currentX || z != currentZ)
                {
                    overlayManager.ChangeOverlay(overlayData);
                    currentX = x;
                    currentZ = z;
                    isOverlayInvisible = false;
                }
            
                if (overlayData.canFarm && isClicked)
                {
                    if (GetFieldState(clickedIndices.Item1, clickedIndices.Item2) == FieldState.NotTilled)
                    {
                        /*
                        if (fieldDecayManager.Tile(x, z, hitTerrain))
                        {
                            print("tiling");
                            AddTilledField(x, z);
                        }*/
                    }
                }
            }
            else
            {
                isOverlayInvisible = true;
                overlayManager.SetOverlayInvisible();

                if (isPlantingMode && state == FieldState.Tilled && isClicked)
                {
                    /*
                    if (fieldDecayManager.Plant(x, z))
                    {
                        print("planting");
                        SetFieldState(x, z, true);
                    }*/
                }
            }
        }
        else
        {
            isOverlayInvisible = true;
            overlayManager.SetOverlayInvisible();
        }
    }

    OverlayData GetOverlayData(int x, int z, Terrain terrain)
    {
        float terrainSizeX = terrain.terrainData.size.x;
        float terrainSizeZ = terrain.terrainData.size.z;
        
        Vector3 centerNormal = Vector3.zero;
            
        bool canFarming = true;
        
        var (startX, startZ) = IndexToGlobal(x, z);
        
        float checkInterval = gridSize / (checkNumSqrt - 1);

        for (int i = 0; i < checkNumSqrt; i++)
        {
            float curX = startX + i * checkInterval;

            for (int j = 0; j < checkNumSqrt; j++)
            {
                float curZ = startZ + j * checkInterval;
                
                float localX = curX - terrain.transform.position.x;
                float localZ = curZ - terrain.transform.position.z;
                
                Vector3 normal = terrain.terrainData.GetInterpolatedNormal(localX / terrainSizeX, localZ / terrainSizeZ);

                if (normal.y < normalThreshold)
                {
                    canFarming = false;
                }
            }
        }

        float centerX = startX + gridSize / 2;
        float centerZ = startZ + gridSize / 2;

        float localCenterX = centerX - terrain.transform.position.x;
        float localCenterZ = centerZ - terrain.transform.position.z;
        
        float centerHeight = GetHeightGlobal(localCenterX, localCenterZ, terrain);
        
        Vector3 center = new Vector3(centerX, centerHeight + 1f, centerZ);
        
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

    public (int, int) GlobalToIdx(float x, float z)
    {
        float relativeX = x - xOffset;
        float relativeZ = z - zOffset;

        int xIndex = Mathf.FloorToInt(relativeX / gridSize);
        int zIndex = Mathf.FloorToInt(relativeZ / gridSize);
        
        return (xIndex, zIndex);
    }

    public (float, float) IndexToGlobal(int x, int z)
    {
        float startX = gridSize * x;
        float startZ = gridSize * z;
        
        return (startX + xOffset, startZ + zOffset);
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
