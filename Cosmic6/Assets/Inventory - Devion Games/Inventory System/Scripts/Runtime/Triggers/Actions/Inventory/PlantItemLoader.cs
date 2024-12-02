using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class PlantItemData
{
    public ItemPrefabData seed;
    public ItemPrefabData fruit;
}

[System.Serializable]
public class ItemPrefabData
{
    public string prefab_path;
    public int cnt;
}

[System.Serializable]
public class PlantItemsDatabase
{
    public Dictionary<string, PlantItemData> Plants;
}

public class PlantItemLoader : MonoBehaviour
{
    public static PlantItemLoader Instance;
    private PlantItemsDatabase plantItemsDatabase;

    void Start()
    {
        LoadPlantItemsDatabase();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadPlantItemsDatabase();
    }

    private void LoadPlantItemsDatabase()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "plant_items.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            plantItemsDatabase = JsonConvert.DeserializeObject<PlantItemsDatabase>(json);
        }
        else
        {
            Debug.LogError("Plant items database not found at: " + filePath);
        }
    }

    public PlantItemData GetPlantItemData(string plantName)
    {
        plantName = plantName.Replace("Seed", "Plant");

        Debug.Log("plantName: "+plantName);
        Debug.Log("Plants: " + plantItemsDatabase.Plants);

        if (plantItemsDatabase != null && plantItemsDatabase.Plants.ContainsKey(plantName))
        {
            return plantItemsDatabase.Plants[plantName];
        }
        else
        {
            Debug.LogError("No Plant Named: " + plantName);
            return null;
        }
    }
}
