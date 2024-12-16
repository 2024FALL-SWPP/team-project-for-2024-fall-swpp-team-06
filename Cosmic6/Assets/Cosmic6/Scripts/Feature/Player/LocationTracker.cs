using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class LocationTracker : MonoBehaviour
{
    public int currentRegionIndex { get; private set; } = 0;
    
    private Vector3[] baseLocations = { new (-1815, 0, -8600), new (22, 0, -9369), new (3240, 0, -10998)};

    public Vector3[] respawnLocations { get; private set; } =
        { new(-1980, 1.65f, -8580), new(-1801.42f, -0.02f, -8694.16f), new(8.6f, 0.65f, -9526f), new(3171.5f, 0f, -10929.1f) };
    private float[] baseRadiusSqs = { 140*140, 150*150, 140*140 };
    public bool isBase { get; private set; } = false;
    public BaseManager baseManager;

    public int lastRespawnIndex = 0; //{ get; private set; } = 0;
    private int terrainLayerIndex = 8;
    private float isBaseCheckInterval = 0.5f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IsBaseCheck());
    }

    IEnumerator IsBaseCheck()
    {
        while (true)
        {
            var nextIsBase = false;
            for (int i = 0; i < baseLocations.Length; i++)
            {
                if (baseManager.isBaseRegistered[i])
                {
                    var xdiff = transform.position.x - baseLocations[i].x;
                    var zdiff = transform.position.z - baseLocations[i].z;
                    var distanceSq = xdiff * xdiff + zdiff * zdiff;

                    if (distanceSq < baseRadiusSqs[i])
                    {
                        lastRespawnIndex = i + 1;
                        nextIsBase = true;
                        break;
                    }
                }
            }
            
            isBase = nextIsBase;
            
            yield return new WaitForSeconds(isBaseCheckInterval);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.layer == terrainLayerIndex)
        {
            var collisionTag = hit.gameObject.tag;
            var idxChar = hit.gameObject.tag[collisionTag.Length - 1];
            currentRegionIndex = idxChar - '1';
        }
    }
}
