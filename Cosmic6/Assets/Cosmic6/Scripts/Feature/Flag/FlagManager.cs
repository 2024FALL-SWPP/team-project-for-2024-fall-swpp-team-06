using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    public bool[] isFlagRegistered { get; private set; } = { false, false, false, false, false };
    public GameObject[] flags;
    public bool isEscapeFlagRegistered = false;
    public GameObject escapeFlag;
    string[] validNames = { "Flag1", "Flag2", "Flag3", "Flag4", "Flag5" };

    // clickable
    private int flagLayerIndex = 3;
    private int flagIndex = 0;
    private bool updateOxygen1 = false;
    private bool updateOxygen2 = false;

    public BaseManager baseManager;
    public InstantMovement instantMovement;
    public CameraRaycaster cameraRaycaster;
    public PlayerStatusController playerStatusController;

    public QuestSystem questSystem;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < flags.Length; i++)
        {
            flags[i].SetActive(false);
        }
        cameraRaycaster.OnRaycastHit += ProcessRaycast;
    }

    void Update()
    {
        switch(flagIndex)
        {
            case 1:
                // tutorial
                break;
            case 2:
                // oxygen at region 2
                if (!updateOxygen1)
                {
                    playerStatusController.UpgradeOxygen();
                    updateOxygen1 = true;
                }
                break;
            case 3:
                // teleport
                instantMovement.teleportPossible = true;
                break;
            case 4:
                // protect heat for region 3
                playerStatusController.isHeatProtected = true;
                break;
            case 5:
                // oxygen at region 3
                if (!updateOxygen2)
                {
                    playerStatusController.UpgradeOxygen();
                    updateOxygen2 = true;
                }
                break;
            default:
                break;
        }
    }

    public void UpdateMinimap()
    {
        for (int i = 0; i < baseManager.isBaseRegistered.Length; i++)
        {
            if (baseManager.isBaseRegistered[i])
            {
                if (i == 0)
                {
                    flags[0].SetActive(true);
                }
                else if (i == 1)
                {
                    flags[1].SetActive(true);
                    flags[2].SetActive(true);
                }
                else
                {
                    flags[3].SetActive(true);
                    flags[4].SetActive(true);
                }
            }
        }
    }

    public void ProcessRaycast(bool isHit, RaycastHit hit, bool isClicked)
    {
        if (!isHit)
        {
            return;
        }

        if (hit.collider.gameObject.layer == flagLayerIndex &&
                System.Array.Exists(validNames, name => name == hit.collider.gameObject.name))
        {
            if (isClicked)
            {
                var collisionName = hit.collider.gameObject.name;
                var idxChar = hit.collider.gameObject.name[collisionName.Length - 1];
                flagIndex = int.Parse(idxChar.ToString());
                Debug.Log($"Flag {flagIndex} detected");
                flags[flagIndex - 1].SetActive(false);

                if (questSystem != null)
                {
                    string variableName = validNames[flagIndex-1];
                    questSystem.UpdateQuestProgress(variableName, 1);
                    Debug.Log($"Quest progress updated for {variableName}");
                }
            }   
        }
        else if (hit.collider.gameObject.layer == flagLayerIndex &&
            hit.collider.gameObject == escapeFlag)
        {
            if (isClicked)
            {
                isEscapeFlagRegistered = true;
                escapeFlag.SetActive(false);

                if (questSystem != null)
                {
                    string variableName = "EscapeFlag";
                    questSystem.UpdateQuestProgress(variableName, 1);
                    Debug.Log($"Quest progress updated for {variableName}");
                }
            }
        }
    }
}
