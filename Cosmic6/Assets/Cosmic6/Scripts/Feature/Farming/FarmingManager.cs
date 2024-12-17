using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevionGames.InventorySystem;

public enum FieldState
{
    NotTilled,  // not cultivated
    Tilled,     // cultivated but not planted
    Planted
}


public class FarmingManager : MonoBehaviour
{

        public static FarmingManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

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
    
    private (int, int) clickedIndices;

    // equip seed
    public bool isPlantingMode = false;
    
    // equip shovel
    public bool isTilingMode = false;

    // hand manager
    public GameObject handManagerObject;
    private HandManager handManager;
    public GameObject plant;
    public bool plantOne;

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
        handManager = handManagerObject.GetComponent<HandManager>();

        isPlantingMode = false;
        isTilingMode = false;
    }
    
    void ExecutePlanting(Vector3 worldPosition)
    {
        // Position만 정하면 Instantiation + Hand에서 숫자 줄어들도록 하는 메서드
        if (isPlantingMode && plant)
        {
            ObjectManager.Instance.SpawnObjectWithName(
                plant,
                plant.name,
                worldPosition,
                Quaternion.identity
            );
            handManager.UseItem(1);
        }
    }
    
    void Update()
    {
        if (handManager.holdItems.Count > 0)
        {
            Item item = handManager.holdItems[0];
            isTilingMode = item.name == "Shovel(Clone)";
            isPlantingMode = item.Prefab.name.Contains("_Plant");
            if (isPlantingMode)
            {
                plant = item.OverridePrefab;
            }
            else
            {
                plant = null;
            }
        }
        else
        {
            isTilingMode = false;
            isPlantingMode = false;
            plant = null;
        }
    }
    
    public void Harvest(float globalX, float globalZ)
    {
        var (xIndex, zIndex) = GlobalToIdx(globalX, globalZ);
        
        var fieldState = fieldDecayManager.Harvest(xIndex, zIndex);
        
        SetFieldState(xIndex, zIndex, fieldState);
        
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
                    if (GetFieldState(x, z) == FieldState.NotTilled)
                    {
                        var fieldState = fieldDecayManager.Tile(x, z, hitTerrain);
                        SetFieldState(x, z, fieldState);
                    }
                }
            }
            else
            {
                isOverlayInvisible = true;
                overlayManager.SetOverlayInvisible();

                if (isPlantingMode && state == FieldState.Tilled && isClicked)
                {                    
                    var fieldState = fieldDecayManager.Plant(x, z);
                    
                    var (globalX, globalZ) = (hitPoint.x, hitPoint.z);
                    var (localX, localZ) = (globalX - hitTerrain.transform.position.x,
                        globalZ - hitTerrain.transform.position.z);
                    
                    var y = GetHeightGlobal(localX, localZ, hitTerrain);
                    
                    ExecutePlanting(new (globalX, y, globalZ));
                    Debug.Log("Planted on" + globalX + " " + globalZ);
                    SetFieldState(x, z, fieldState);
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
    
    public bool SetFieldState(int x, int z, FieldState state)
    {
        if (state != FieldState.NotTilled)
        {
            fieldStates[(x, z)] = state;
        } else {
            fieldStates.Remove((x, z));
        }
        
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