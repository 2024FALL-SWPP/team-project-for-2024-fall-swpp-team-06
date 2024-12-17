using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DevionGames.InventorySystem;

public class PlantLoader : MonoBehaviour
{
    [System.Serializable]
    public class PlantData
    {
        public string name;
        public Vector3 position;
    }

    [System.Serializable]
    public class PlantDataList
    {
        public List<PlantData> plants;
    }

    public string jsonFilePath = "Assets/StreamingAssets/default_plants.json";
    public float spawnInterval = 0.05f;

    private PlantDataList loadedPlantData;
    public GameObject plantsParent;

    void Start()
    {
        LoadPlantData();
        if (loadedPlantData != null && loadedPlantData.plants.Count > 0)
        {
            StartCoroutine(SpawnPlantsCoroutine());
        }
        else
        {
            Debug.LogError("Failed to load plant data or the list is empty.");
        }
    }

    void LoadPlantData()
    {
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            loadedPlantData = JsonUtility.FromJson<PlantDataList>(json);
            Debug.Log("Successfully loaded plant data from " + jsonFilePath);
        }
        else
        {
            Debug.LogError("JSON file not found at: " + jsonFilePath);
        }
    }

    IEnumerator SpawnPlantsCoroutine()
    {
        foreach (PlantData plantData in loadedPlantData.plants)
        {
            string prefabPath = "Assets/Cosmic6/Prefabs12DB/" + plantData.name + ".prefab";
            GameObject plantPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (plantPrefab != null)
            {
                GameObject spawnedPlant = ObjectManager.Instance.SpawnObjectWithName(plantPrefab, plantData.name, plantData.position, Quaternion.identity);
                spawnedPlant.transform.SetParent(plantsParent.transform);
                Debug.Log("Spawned: " + plantData.name + " at " + plantData.position);
            }
            else
            {
                Debug.LogWarning("Prefab not found for: " + prefabPath);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
        Debug.Log("Finished spawning all plants.");
    }
}
