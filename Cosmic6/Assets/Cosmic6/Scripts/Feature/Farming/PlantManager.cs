using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using DevionGames.InventorySystem;
using System;

public class PlantManager : MonoBehaviour
{
    public class PlantData
    {
        public string plantName;
        public Vector3 position;
        public PlantLifecycle plantLifecycle;

        public PlantData(string name, Vector3 pos, PlantLifecycle lifecycle)
        {
            plantName = name;
            position = pos;
            plantLifecycle = lifecycle;
        }
    }

    public Action OnHarvesting;
    public LocationTracker locationTracker;
    private PlantLoader plantLoader;
    private List<PlantData> plantList = new List<PlantData>();
    private Dictionary<string, int> plantNameCounts = new Dictionary<string, int>();
    private int currentIndex;
    private float checkRate = 0.1f;

    public static PlantManager Instance;
    

    private void Awake()
    {
        plantLoader = GetComponent<PlantLoader>();
        
        plantLoader.OnPlantsLoaded += StartTrackingLocation;
        currentIndex = locationTracker.currentRegionIndex;
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    // Add plant data
    public string AddPlant(string name, Vector3 pos, PlantLifecycle lifecycle)
    {
        if (!plantNameCounts.ContainsKey(name))
        {
            plantNameCounts[name] = 0;
        }
        plantNameCounts[name]++;

        string uniqueName = $"{name}_{plantNameCounts[name]}";
        PlantData newPlant = new PlantData(uniqueName, pos, lifecycle);
        plantList.Add(newPlant);
        Debug.Log($"식물 추가: {uniqueName}, 위치: {pos}");
        return uniqueName;
    }

    public void StartTrackingLocation()
    {
        for (int i = 0; i < plantLoader.plantsParents.Length; i++)
        {
            if (i != currentIndex)
            {
                plantLoader.plantsParents[i].SetActive(false);
            }
        }
        
        StartCoroutine(TrackingLocation());
    }

    IEnumerator TrackingLocation()
    {
        while (true)
        {
            if (Math.Min(locationTracker.currentRegionIndex, 2) != currentIndex)
            {
                plantLoader.plantsParents[currentIndex].SetActive(false);
                currentIndex = Math.Min(locationTracker.currentRegionIndex, 2);
                plantLoader.plantsParents[currentIndex].SetActive(true);
            }
            
            
            yield return new WaitForSeconds(checkRate);
        }
    }

    // Check harvest
    private void CheckHarvest()
    {
        for (int i = plantList.Count - 1; i >= 0; i--)
        {
            PlantData plant = plantList[i];

            if (plant.plantLifecycle && plant.plantLifecycle.plantStatus == "Plant_3")
            {
                // TODO: Substitute the plant object with Prefab
                string plantName = plant.plantName.Split("_")[0] + "_" + plant.plantName.Split("_")[1];
                string plantFullName = plant.plantName;
                Debug.Log($"수확 가능한 식물 발견: {plant.plantName}, 위치: {plant.position}");

                PlantItemData plantItemData = PlantItemLoader.Instance.GetPlantItemData(plantName);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(plantItemData.seed.prefab_path);

                GameObject plantPrefab = ObjectManager.Instance.SpawnObjectWithName(prefab, plantFullName, plant.position, Quaternion.identity);
                ItemCollection itemCollection = plantPrefab.GetComponent<ItemCollection>();
                itemCollection.name = plantFullName;

                Debug.Log("식물 교체 완: " + plantPrefab.name);

                Destroy(plant.plantLifecycle.gameObject);
            }
        }
    }

    // Remove plant by name
    private void RemovePlant(string plantName)
    {
        for (int i = plantList.Count - 1; i >= 0; i--)
        {
            if (plantList[i].plantName == plantName)
            {
                // FarmingManager로 제거된 식물 position 보내기
                Vector3 position = plantList[i].position;
                FarmingManager.Instance.Harvest(position.x, position.z);

                Debug.Log($"Plant Removed: {plantList[i].plantName}, Position: {plantList[i].position}");
                plantList.RemoveAt(i);
                return;
            }
        }
        Debug.Log($"Plant {plantName} Not Found");
    }

    // Remove all objects with "(Log)" in the name
    private void RemoveLogObjects()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("(Log)"))
            {
                string cleanedName = obj.name.Replace("(Log) ", "");
                RemovePlant(cleanedName);
                Debug.Log($"오브젝트 제거: {obj.name}");
                Destroy(obj);
            }
        }
    }

    void Update()
    {
        RemoveLogObjects();
    }

    void LateUpdate()
    {
        CheckHarvest();
    }
}