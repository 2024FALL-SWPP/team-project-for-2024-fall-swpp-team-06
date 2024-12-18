using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

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

    [System.Serializable]
    public class PlantDataList
    {
        public List<PlantData> plants = new List<PlantData>();
    }

    [System.Serializable]
    public class PlantData
    {
        public string name;
        public int terrainIndex;
        public Vector3 position;
    }

    public GameObject terrain1;
    public GameObject terrain2;
    public GameObject terrain3;

    private int minPlants = 5;
    private int spawnNumPeriod = 5;
    private float timeBetweenSpawns = 0.05f;

    public List<TerrainData> terrains1 = new List<TerrainData>();
    public List<TerrainData> terrains2 = new List<TerrainData>();
    public List<TerrainData> terrains3 = new List<TerrainData>();
    
    private PlantDataList plantDataList = new PlantDataList();

    static List<string> terrain1PrefabPaths = new List<string> {
        "Assets/Cosmic6/Prefabs/Plants/1_Plant",
        "Assets/Cosmic6/Prefabs/Plants/2_Plant",
        "Assets/Cosmic6/Prefabs/Plants/3_Plant",
        "Assets/Cosmic6/Prefabs/Plants/4_Plant",
        "Assets/Cosmic6/Prefabs/Plants/5_Plant",
    };
    static List<string> terrain2PrefabPaths = new List<string> {
        "Assets/Cosmic6/Prefabs/Plants/6_Plant",
        "Assets/Cosmic6/Prefabs/Plants/7_Plant",
        "Assets/Cosmic6/Prefabs/Plants/11_Plant",
        "Assets/Cosmic6/Prefabs/Plants/12_Plant",
    };
    static List<string> terrain3PrefabPaths = new List<string> {
        "Assets/Cosmic6/Prefabs/Plants/8_Plant",
        "Assets/Cosmic6/Prefabs/Plants/9_Plant",
        "Assets/Cosmic6/Prefabs/Plants/10_Plant",
        "Assets/Cosmic6/Prefabs/Plants/13_Plant",
        "Assets/Cosmic6/Prefabs/Plants/14_Plant",
        "Assets/Cosmic6/Prefabs/Plants/15_Plant"
    };

    public void GenerateTerrains()
    {
        terrains1.Clear();
        terrains2.Clear();
        terrains3.Clear();
        ProcessTerrain(terrain1, terrain1PrefabPaths, terrains1);
        ProcessTerrain(terrain2, terrain2PrefabPaths, terrains2);
        ProcessTerrain(terrain3, terrain3PrefabPaths, terrains3);
    }

    void ProcessTerrain(GameObject terrainObject, List<string> prefabPaths, List<TerrainData> terrainDataList)
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

                terrainDataList.Add(new TerrainData
                {
                    spawnArea = new Rect(position.x, position.z, sizeX, sizeZ),
                    prefabPaths = prefabPaths,
                    minPlants = minPlants,
                    terrain = terrainComponent
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
        foreach (var terrain in terrains1)
        {
            int spawnedCount = 0;
            while (spawnedCount < terrain.minPlants)
            {
                Vector3 randomPosition;
                OverlayData overlayData;

                do
                {
                    randomPosition = GenerateRandomPosition(terrain);
                    var (xIndex, zIndex) = FarmingManager.Instance.GlobalToIdx(randomPosition.x, randomPosition.z);
                    overlayData = FarmingManager.Instance.GetOverlayData(xIndex, zIndex, terrain.terrain);
                }
                while (!(IsPositionValid(randomPosition, terrain) && overlayData.canFarm));

                SpawnPlant(overlayData.position, terrain, 0, initialSpawn: true);
                spawnedCount++;
                yield return null;
            }
        }
        foreach (var terrain in terrains2)
        {
            int spawnedCount = 0;
            while (spawnedCount < terrain.minPlants)
            {
                Vector3 randomPosition;
                OverlayData overlayData;

                do
                {
                    randomPosition = GenerateRandomPosition(terrain);
                    var (xIndex, zIndex) = FarmingManager.Instance.GlobalToIdx(randomPosition.x, randomPosition.z);
                    overlayData = FarmingManager.Instance.GetOverlayData(xIndex, zIndex, terrain.terrain);
                }
                while (!(IsPositionValid(randomPosition, terrain) && overlayData.canFarm));

                SpawnPlant(overlayData.position, terrain, 1, initialSpawn: true);
                spawnedCount++;
                yield return null;
            }
        }
        foreach (var terrain in terrains3)
        {
            int spawnedCount = 0;
            while (spawnedCount < terrain.minPlants)
            {
                Vector3 randomPosition;
                OverlayData overlayData;

                do
                {
                    randomPosition = GenerateRandomPosition(terrain);
                    var (xIndex, zIndex) = FarmingManager.Instance.GlobalToIdx(randomPosition.x, randomPosition.z);
                    overlayData = FarmingManager.Instance.GetOverlayData(xIndex, zIndex, terrain.terrain);
                }
                while (!(IsPositionValid(randomPosition, terrain) && overlayData.canFarm));

                SpawnPlant(overlayData.position, terrain, 2, initialSpawn: true);
                spawnedCount++;
                yield return null;
            }
        }
        SavePlantDataToJson("Assets/StreamingAssets/default_plants.json");
    }

    Vector3 GenerateRandomPosition(TerrainData terrain)
    {
        float x = Random.Range(terrain.spawnArea.xMin, terrain.spawnArea.xMax);
        float z = Random.Range(terrain.spawnArea.yMin, terrain.spawnArea.yMax);
        return new Vector3(x, 0, z);
    }

    void SpawnPlant(Vector3 position, TerrainData terrain, int terrainIndex, bool initialSpawn = false)
    {
        string prefabPath = terrain.prefabPaths[Random.Range(0, terrain.prefabPaths.Count)];
        if (initialSpawn)
        {
            prefabPath = prefabPath.Replace("Assets/Cosmic6/Prefabs/Plants/", "Assets/Cosmic6/Prefabs12DB/");
        }
        GameObject plantPrefab = Resources.Load<GameObject>(prefabPath);
        if (plantPrefab != null)
        {
            ObjectManager.Instance.SpawnObjectWithName(plantPrefab, plantPrefab.name, position, Quaternion.identity);
            spawnedPositions.Add(position);
            plantDataList.plants.Add(new PlantData { name = plantPrefab.name, terrainIndex=terrainIndex, position = position });
        }
    }

    void SavePlantDataToJson(string filePath)
    {
        if (plantDataList.plants.Count > 0)
        {
            string json = JsonUtility.ToJson(plantDataList, true);
            File.WriteAllText(filePath, json);
            Debug.Log("Saved plant data to " + filePath);
        }
        else
        {
            Debug.LogWarning("Plant data list is empty. No data saved.");
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
