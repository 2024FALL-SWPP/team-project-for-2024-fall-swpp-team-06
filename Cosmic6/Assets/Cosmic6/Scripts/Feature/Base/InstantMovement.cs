using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantMovement : MonoBehaviour
{
    public PlayerState playerState;
    public FlagState flagState;
    public Transform playerTransform;

    public Collider base1Collider;
    public Collider base2Collider;
    public Collider base3Collider;

    public Transform base1TeleportPoint;
    public Transform base2TeleportPoint;
    public Transform base3TeleportPoint;

    private Collider currentBaseCollider;

    private const float teleportRange = 5.0f;

    /*public void TeleportToBase(int fromBase, int toBase)
    {
        if (fromBase < 1 || fromBase > 3 || toBase < 1 || toBase > 3)
        {
            Debug.LogError("Invalid base nubmer");
            return;
        }

        bool fromRegistered = GetRegistrationStatus(fromBase);
        bool toRegistered = GetRegistrationStatus(toBase);

        if (!fromRegistered && !toRegistered)
        {
            Debug.LogError("Teleportation not possible: Both bases must be registered");
            return;
        }

        if (IsValidTeleportPath(fromBase, toBase))
        {
            Debug.Log($"Teleporting from Base{fromBase} to Base{toBase}.");
            TeleportPlayer(toBase);
        }
        else
        {
            Debug.LogError("Teleportation ont possible: Invalid path");
        }
    }*/

    /*private bool GetRegistrationStatus(int baseNumber)
    {
        return baseNumber switch
        {
            1 => playerState.isRegistered1,
            2 => playerState.isRegistered2,
            3 => playerState.isRegistered3,
            _ => false
        };
    }

    private bool IsValidTeleportPath(int fromBase, int toBase)
    {
        return (fromBase == 1 && toBase == 2) ||
               (fromBase == 2 && toBase == 1) ||
               (fromBase == 2 && toBase == 3) ||
               (fromBase == 3 && toBase == 2) ||
               (fromBase == 1 && toBase == 3) ||
               (fromBase == 3 && toBase == 1);
    }*/

    private void TeleportPlayer(int toBase)
    {
        Transform targetTeleportPoint = toBase switch
        {
            1 => base1TeleportPoint,
            2 => base2TeleportPoint,
            3 => base3TeleportPoint,
            _ => null
        };

        if (targetTeleportPoint == null)
        {
            Debug.LogError($"Invalid target teleport point for Base{toBase}. Cannot teleport.");
            return;
        }

        playerTransform.position = targetTeleportPoint.position;
        Debug.Log($"Player teleported to Base{toBase} at position {targetTeleportPoint.position}.");

        /*Vector3 closestPoint = GetSafeClosestPoint(targetCollider);

        Vector3 finalPosition = AdjustToGround(closestPoint + Vector3.up * 1.0f);

        playerTransform.position = finalPosition + Vector3.left * 10.0f;
        Debug.Log($"Player teleported to Base{toBase} at position {finalPosition}.");*/
    }

    /*private Vector3 GetSafeClosestPoint(Collider baseCollider)
    {
        Vector3 closestPoint = baseCollider.ClosestPoint(playerTransform.position);

        Debug.Log($"ClosestPoint calculated as: {closestPoint} for base: {baseCollider.name}");
        
        if (closestPoint == Vector3.zero)
        {
            Debug.LogWarning("ClosestPoint returned an invalid positoin. Using base's default position.");
            return baseCollider.transform.position + Vector3.up * 1.0f;
        }
        return closestPoint;
    }

    private Vector3 AdjustToGround(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * 5.0f, Vector3.down, out RaycastHit hit, 10.0f))
        {
            return hit.point;   // return position on the ground
        }

        Debug.LogWarning("Failed to adjust to ground, Returning origianl position");
        return position;
    }*/

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckNearbyBase();

        // 'T' input
        if (Input.GetKeyDown(KeyCode.T) && currentBaseCollider != null)
        {
            ShowTeleportOptions();
        }
    }

    private void CheckNearbyBase()
    {
        currentBaseCollider = null;

        if (IsPlayerWithinRange(base1Collider)) currentBaseCollider = base1Collider;
        else if (IsPlayerWithinRange(base2Collider)) currentBaseCollider = base2Collider;
        else if (IsPlayerWithinRange(base3Collider)) currentBaseCollider = base3Collider;
    }

    private bool IsPlayerWithinRange(Collider baseCollider)
    {
        // calculate the closest point from player position to base collider using ClosestPoint()
        Vector3 clsoestPoint = baseCollider.ClosestPoint(playerTransform.position);
        
        // calcualte distance between closestPoint and player's position
        float distance = Vector3.Distance(playerTransform.position, clsoestPoint);
        
        return distance <= teleportRange;
    }

    private void ShowTeleportOptions()
    {
        Debug.Log("Nearby Base detected. Choose a destination.");

        if (currentBaseCollider = base1Collider)
        {
            if (playerState.isRegistered2) Debug.Log("Press 2 to teleport to Base2.");
            if (playerState.isRegistered3) Debug.Log("Press 3 to teleport to Base3.");
        }
        else if (currentBaseCollider = base2Collider)
        {
            if (playerState.isRegistered1) Debug.Log("Press 1 to teleport to Base1.");
            if (playerState.isRegistered3) Debug.Log("Press 3 to teleport to Base3.");
        }
        else if (currentBaseCollider = base3Collider)
        {
            if (playerState.isRegistered1) Debug.Log("Press 1 to teleport to Base1.");
            if (playerState.isRegistered2) Debug.Log("Press 2 to teleport to Base2.");
        }

        StartCoroutine(WaitForTeleportInput());
    }

    private System.Collections.IEnumerator WaitForTeleportInput()
    {
        bool teleportDone = false;

        while (!teleportDone)
        {
            if (currentBaseCollider == base1Collider)
            {
                if (Input.GetKeyDown(KeyCode.Alpha2) && playerState.isRegistered2)
                {
                    TeleportPlayer(2);
                    teleportDone = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3) && playerState.isRegistered3)
                {
                    TeleportPlayer(3);
                    teleportDone = true;
                }
            }
            else if (currentBaseCollider == base2Collider)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) && playerState.isRegistered1)
                {
                    TeleportPlayer(1);
                    teleportDone = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3) && playerState.isRegistered3)
                {
                    TeleportPlayer(3);
                    teleportDone = true;
                }
            }
            else if (currentBaseCollider == base3Collider)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) && playerState.isRegistered1)
                {
                    TeleportPlayer(1);
                    teleportDone = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2) && playerState.isRegistered2)
                {
                    TeleportPlayer(2);
                    teleportDone = true;
                }
            }

            yield return null;
        }
    }
}
