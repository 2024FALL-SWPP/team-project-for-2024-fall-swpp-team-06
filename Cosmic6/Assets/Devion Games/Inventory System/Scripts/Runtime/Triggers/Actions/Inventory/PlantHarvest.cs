using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantHarvest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        Vector3 position = transform.position;
        Debug.Log($"Object {gameObject.name} clicked at position: {position}");
    }
}
