using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantMovement : MonoBehaviour
{
    public event Action OnTeleport;

    public Transform playerTransform;
    public BaseManager baseManager;
    public bool teleportPossible = false;
    public int targetBaseIndex = -1;

    public GameObject[] basePositions;
    public GameObject teleportPopupUI;
    public TMPro.TMP_Text teleportInstructions;

    private const float teleportRange = 5.0f;
    private int currentBaseIndex = -1;
    private bool isTeleporting = false;
    private bool isTeleportingMenuActive = false;

    private void Start()
    {
        teleportPopupUI.SetActive(false);
        for (int i = 0; i < baseManager.isBaseRegistered.Length; i++)
        {
            basePositions[i].SetActive(false);
        }
    }

    private void Update()
    {
        if (teleportPossible)
        {
            print("teleport is possible");
            UpdateMapMarkers();
            if (!isTeleportingMenuActive) { UpdateCurrentBase(); }

            if (Input.GetKeyDown(KeyCode.T) && currentBaseIndex != -1)
            {
                isTeleportingMenuActive = true;
                ShowTeleportOptions();
            }
        }
    }

    private void UpdateMapMarkers()
    {
        for (int i = 0; i < baseManager.isBaseRegistered.Length; i++)
        {
            print(baseManager.isBaseRegistered[i]);
            basePositions[i].SetActive(baseManager.isBaseRegistered[i]);
        }
    }

    private void UpdateCurrentBase()
    {
        currentBaseIndex = -1;

        for (int i = 0; i < baseManager.isBaseRegistered.Length; i++)
        {
            if (!baseManager.isBaseRegistered[i]) continue;

            if (CheckDistance(i) <= teleportRange)
            {
                currentBaseIndex = i;
                teleportPopupUI.SetActive(true);
                teleportInstructions.text = $"Press 'T'\nto teleport\nother bases.";
                break;
            }
            else
            {
                teleportPopupUI.SetActive(false);
            }
        }
    }

    private void ShowTeleportOptions()
    {
        teleportPopupUI.SetActive(true);
        string destinations = "";

        for (int i = 0; i < baseManager.isBaseRegistered.Length; i++)
        {
            if (i == currentBaseIndex) continue;
            if (baseManager.isBaseRegistered[i])
            {
                if (!string.IsNullOrEmpty(destinations))
                {
                    destinations += ", ";
                }
                destinations += $"{i + 1}";
            }
        }

        teleportInstructions.text = $"Currently near base {currentBaseIndex + 1}.\n" +
            $"Choose a teleport destination: {destinations}";

        StartCoroutine(WaitForTeleportInput());
    }

    private IEnumerator WaitForTeleportInput()
    {
        while (!isTeleporting)
        {
            for (int i = 0; i < baseManager.isBaseRegistered.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) && baseManager.isBaseRegistered[i] && i != currentBaseIndex)
                {
                    targetBaseIndex = i;
                    TeleportPlayer();
                    teleportPopupUI.SetActive(false);
                    isTeleportingMenuActive = false;
                    yield break;
                }

                if (CheckDistance(currentBaseIndex) > teleportRange)
                {
                    teleportPopupUI.SetActive(false);
                    isTeleportingMenuActive = false;
                    yield break;
                }
            }

            yield return null;
        }
    }

    private void TeleportPlayer()
    {
        OnTeleport?.Invoke();
        isTeleporting = false;
        currentBaseIndex = -1;
        targetBaseIndex = -1;
    }

    private float CheckDistance(int idx)
    {
        Vector3 basePosition = basePositions[idx].transform.position;
        float distance = Vector3.Distance(playerTransform.position, basePosition);
        return distance;
    }
}