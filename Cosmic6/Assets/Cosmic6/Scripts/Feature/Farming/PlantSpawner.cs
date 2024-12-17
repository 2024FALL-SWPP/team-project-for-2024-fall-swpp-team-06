using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlantSpawner : MonoBehaviour
{
    [System.Serializable]
    public class TerrainData
    {
        public Rect spawnArea;
        public List<string> prefabPaths;
        public int minPlants;
        public Terrain terrain;
    }

    public GameObject terrain1;
    public GameObject terrain2;
    public GameObject terrain3;

    private int minPlants = 15;
    private int spawnNumPeriod = 5;
    private float timeBetweenSpawns = 0.05f;

    public List<TerrainData> terrains = new List<TerrainData>();

    
    static List<string> terrain1PrefabPaths = new List<string> {
        "Assets/Cosmic6/Prefabs/Plants/1_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/2_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/3_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/4_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/5_Plant.prefab",
    };
    static List<string> terrain2PrefabPaths = new List<string> {
        "Assets/Cosmic6/Prefabs/Plants/6_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/7_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/11_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/12_Plant.prefab",
    };
    static List<string> terrain3PrefabPaths = new List<string> {
        "Assets/Cosmic6/Prefabs/Plants/8_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/9_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/10_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/13_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/14_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/15_Plant.prefab"
    };


    public void GenerateTerrains()
    {
        terrains.Clear();
        ProcessTerrain(terrain1, terrain1PrefabPaths);
        ProcessTerrain(terrain2, terrain2PrefabPaths);
        ProcessTerrain(terrain3, terrain3PrefabPaths);
    }

    void ProcessTerrain(GameObject terrainObject, List<string> prefabPaths)
    {
        if (terrainObject == null) return;

        foreach (Transform child in terrainObject.transform)
        {
            Terrain terrainComponent = child.GetComponent<Terrain>();
            if (terrainComponent != null)
            {
                Vector3 position = child.position;
                float sizeX = terrainComponent.terrainData.size.x;
                float sizeZ = terrainComponent.terrainData.size.z;

                
                terrains.Add(new TerrainData
                {
                    spawnArea = new Rect(position.x, position.z, sizeX, sizeZ),
                    prefabPaths = prefabPaths,
                    minPlants = minPlants,
                    terrain = child.GetComponent<Terrain>()
                });
            }
        }
    }


    public float minDistance = 40f;

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
    {
        GenerateTerrains();
        StartCoroutine(InitialRandomSpawn());
    }

    IEnumerator InitialRandomSpawn()
    {
        foreach (var terrain in terrains)
        {
            int spawnedCount = 0;
            while (spawnedCount < terrain.minPlants)
            {
                Vector3 randomPosition = GenerateRandomPosition(terrain);
                
                Vector3 newPosition;
                OverlayData overlayData;
                
                do
                {
                    newPosition = GenerateRandomPosition(terrain);
                    var (xIndex, zIndex) = FarmingManager.Instance.GlobalToIdx(newPosition.x, newPosition.z);
            
                    overlayData = FarmingManager.Instance.GetOverlayData(xIndex, zIndex, terrain.terrain);
                }
                while (!(IsPositionValid(newPosition, terrain) && overlayData.canFarm));
                
                if (IsPositionValid(randomPosition, terrain))
                {
                    SpawnPlant(overlayData.position, terrain, initialSpawn: true);
                    spawnedCount++;
                }

                yield return null;
            }
        }
    }

    Vector3 GenerateRandomPosition(TerrainData terrain)
    {
        float x = Random.Range(terrain.spawnArea.xMin, terrain.spawnArea.xMax);
        float z = Random.Range(terrain.spawnArea.yMin, terrain.spawnArea.yMax);
        return new Vector3(x, 0, z);
    }

    public void SubstitutePlant(Vector3 oldPosition, TerrainData terrain)
    {
        spawnedPositions.Remove(oldPosition);

        Vector3 newPosition;
        OverlayData overlayData;
        int attempts = 0;
        do
        {
            newPosition = GenerateRandomPosition(terrain);
            var (xIndex, zIndex) = FarmingManager.Instance.GlobalToIdx(newPosition.x, newPosition.z);
            
            overlayData = FarmingManager.Instance.GetOverlayData(xIndex, zIndex, terrain.terrain);
            attempts++;
        }
        while (!(IsPositionValid(newPosition, terrain) && overlayData.canFarm));

        if (IsPositionValid(newPosition, terrain) && overlayData.canFarm)
        {
            SpawnPlant(overlayData.position, terrain);
        }
    }

    void SpawnPlant(Vector3 position, TerrainData terrain, bool initialSpawn = false)
    {
        // Spawning Plant
        string prefabPath = terrain.prefabPaths[Random.Range(0, terrain.prefabPaths.Count)];
        if (initialSpawn)
        {
            prefabPath = prefabPath.Replace("Assets/Cosmic6/Prefabs/Plants/", "Assets/Cosmic6/Prefabs12DB/");
        }
        GameObject plantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        Debug.Log("Spawning Plant: " + position + " " + prefabPath + ": "+plantPrefab);

        if (plantPrefab != null)
        {
            ObjectManager.Instance.SpawnObjectWithName(plantPrefab, plantPrefab.name, position, Quaternion.identity);
            spawnedPositions.Add(position);
        }
    }

    bool IsPositionValid(Vector3 position, TerrainData terrain)
    {
        foreach (var spawnedPosition in spawnedPositions)
        {
            if (Vector3.Distance(position, spawnedPosition) < minDistance)
            {
                return false;
            }
        }
        return true;
    }

}
