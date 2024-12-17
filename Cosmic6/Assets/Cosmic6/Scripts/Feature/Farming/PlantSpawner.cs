using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
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

    static List<string> rect1PrefabPaths = new List<string> {
        "Assets/Cosmic6/Prefabs/Plants/1_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/2_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/3_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/4_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/5_Plant.prefab",
    };
    static List<string> rect2PrefabPaths = new List<string> {
        "Assets/Cosmic6/Prefabs/Plants/6_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/7_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/11_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/12_Plant.prefab",
    };
    static List<string> rect3PrefabPaths = new List<string> {
        "Assets/Cosmic6/Prefabs/Plants/8_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/9_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/10_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/13_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/14_Plant.prefab",
        "Assets/Cosmic6/Prefabs/Plants/15_Plant.prefab"
    };

    public List<TerrainData> terrains = new List<TerrainData>
    {
        // Rect 1: Split into 1000x1000 squares
        new TerrainData { spawnArea = new Rect(-4500, -7500, 1000, 1000), prefabPaths = rect1PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(-3500, -7500, 1000, 1000), prefabPaths = rect1PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(-2500, -7500, 1000, 1000), prefabPaths = rect1PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(-1500, -7500, 1000, 1000), prefabPaths = rect1PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(-4500, -6500, 1000, 1000), prefabPaths = rect1PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(-3500, -6500, 1000, 1000), prefabPaths = rect1PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(-2500, -6500, 1000, 1000), prefabPaths = rect1PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(-1500, -6500, 1000, 1000), prefabPaths = rect1PrefabPaths, minPlants = 500 },

        // Rect 2: Split into 1000x1000 squares
        new TerrainData { spawnArea = new Rect(-500, -11500, 1000, 1000), prefabPaths = rect2PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(500, -11500, 1000, 1000), prefabPaths = rect2PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(-500, -10500, 1000, 1000), prefabPaths = rect2PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(500, -10500, 1000, 1000), prefabPaths = rect2PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(-500, -9500, 1000, 1000), prefabPaths = rect2PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(500, -9500, 1000, 1000), prefabPaths = rect2PrefabPaths, minPlants = 500 },

        // Rect 3: Split into 1000x1000 squares
        new TerrainData { spawnArea = new Rect(2500, -14500, 1000, 1000), prefabPaths = rect3PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(3500, -14500, 1000, 1000), prefabPaths = rect3PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(4500, -14500, 1000, 1000), prefabPaths = rect3PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(2500, -13500, 1000, 1000), prefabPaths = rect3PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(3500, -13500, 1000, 1000), prefabPaths = rect3PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(4500, -13500, 1000, 1000), prefabPaths = rect3PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(2500, -12500, 1000, 1000), prefabPaths = rect3PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(3500, -12500, 1000, 1000), prefabPaths = rect3PrefabPaths, minPlants = 500 },
        new TerrainData { spawnArea = new Rect(4500, -12500, 1000, 1000), prefabPaths = rect3PrefabPaths, minPlants = 500 }
    };

    public float minDistance = 40f;

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
    {
        InitialRandomSpawn();
    }

    void InitialRandomSpawn()
    {
        foreach (var terrain in terrains)
        {
            int spawnedCount = 0;
            while (spawnedCount < terrain.minPlants)
            {
                Vector3 randomPosition = GenerateRandomPosition(terrain);
                if (IsPositionValid(randomPosition, terrain))
                {
                    SpawnPlant(randomPosition, terrain, initialSpawn: true);
                    spawnedCount++;
                }
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
            overlayData = FarmingManager.Instance.GetOverlayData((int)newPosition.x, (int)newPosition.z, terrain.terrain);
            attempts++;
        }
        while (!(IsPositionValid(newPosition, terrain) && overlayData.canFarm) && attempts < terrain.minPlants * 2);

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
