using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Instance;

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

    public GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        //Debug.Log("Object successfully instantiated!");

        return Instantiate(prefab, position, rotation);
    }

    public GameObject SpawnObjectWithName(GameObject prefab, string prefabName, Vector3 position, Quaternion rotation)
    {
        //Debug.Log("Object successfully instantiated!");

        GameObject generatedPrefab = Instantiate(prefab, position, rotation);
        generatedPrefab.name = prefabName;
        return generatedPrefab;
    }

    public void RemoveObject(GameObject obj, float delay = 0f)
    {
        Destroy(obj, delay);
    }
}