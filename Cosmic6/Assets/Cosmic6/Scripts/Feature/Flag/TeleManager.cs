using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleManager : MonoBehaviour
{
    public GameObject[] teleFlags;
    public GameObject escapeFlag;
    string[] validNames = { "Tele1", "Tele2", "Tele3", "Tele4", "Tele5", "Tele6", "Tele7" };

    public bool[] isTeleFound { get; private set; } = { false, false, false, false, false, false, false };

    private int teleNum = 0;
    public int teleRegion1 = 0;
    public int teleRegion2 = 0;
    public int teleRegion3 = 0;

    // clickable
    private int teleLayerIndex = 3;

    public CameraRaycaster cameraRaycaster;
    public QuestSystem questSystem;

    // Start is called before the first frame update
    void Start()
    {
        cameraRaycaster.OnRaycastHit += ProcessRaycast;
        escapeFlag.SetActive(false);
    }

    private void Update()
    {
        
    }

    public void ProcessRaycast(bool isHit, RaycastHit hit, bool isClicked)
    {
        if (!isHit)
        {
            return;
        }

        if (hit.collider.gameObject.layer == teleLayerIndex &&
                System.Array.Exists(validNames, name => name == hit.collider.gameObject.name))
        {
            if (isClicked)
            {
                var collisionName = hit.collider.gameObject.name;
                var idxChar = hit.collider.gameObject.name[collisionName.Length - 1];
                int idx = int.Parse(idxChar.ToString());
                UpdateTeleNum(idx);
            }
        }
    }

    private void UpdateTeleNum(int idx)
    {
        teleNum++;
        questSystem.UpdateQuestProgress("Tele", 1);
        if (idx < 4 && idx > 0) { teleRegion1++; }
        else if (idx < 6) { teleRegion2++; }
        else { teleRegion3++; }
        teleFlags[idx - 1].SetActive(false);
        isTeleFound[idx - 1] = true;
        Debug.Log($"tele{idx} is deleted. {7 - teleNum} teles remain.");

        if (teleNum == 7)
        {
            escapeFlag.SetActive(true);

            if (questSystem != null)
            {
                string variableName = "EscapeFlag";
                questSystem.UpdateQuest(variableName, 1);
                Debug.Log($"Quest updated for {variableName}");
            }
        }
    }
}
