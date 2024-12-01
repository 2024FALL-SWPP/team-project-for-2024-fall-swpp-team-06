using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleDetector : MonoBehaviour
{
    public Transform playerTransform;
    public TeleManager teleManager;
    public float detectionRange = 150.0f;
    public float detectionInterval = 1.0f;

    private void Start()
    {
        StartCoroutine(DetectNearbyTeleportCoroutine());
    }

    private void Update()
    {
        
    }

    private IEnumerator DetectNearbyTeleportCoroutine()
    {
        while (true)
        {
            DetectNearbyTeleports();
            yield return new WaitForSeconds(detectionInterval);
        }
    }

    private void DetectNearbyTeleports()
    {
        GameObject nearbyTeleport = null;

        foreach(var tele in teleManager.teleFlags)
        {
            if (tele == null) continue;

            float distance = Vector3.Distance(playerTransform.position, tele.transform.position);

            if (distance <= detectionRange)
            {
                nearbyTeleport = tele;
                break;
            }
        }

        if (nearbyTeleport != null)
        {
            Debug.Log($"Nearby Tele: {nearbyTeleport.name}");
        }
        else
        {
            Debug.Log("No Tele nearby");
        }
    }
}
