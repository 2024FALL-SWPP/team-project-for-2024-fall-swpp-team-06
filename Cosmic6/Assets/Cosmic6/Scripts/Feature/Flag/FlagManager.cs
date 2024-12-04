using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    public bool[] isFlagRegistered { get; private set; } = { false, false, false, false, false };
    public GameObject[] flags;
    string[] validNames = { "Flag1", "Flag2", "Flag3", "Flag4", "Flag5" };

    // clickable
    private int flagLayerIndex = 3;
    private int flagIndex = 0;

    public InstantMovement instantMovement;
    public CameraRaycaster cameraRaycaster;

    public QuestSystem questSystem;

    // Start is called before the first frame update
    void Start()
    {
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
                break;
            case 3:
                // protect heat for region 3
                break;
            case 4:
                // oxygen at region 3
                break;
            case 5:
                // teleport
                instantMovement.teleportPossible = true;
                break;
            default:
                break;
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
    }
}
