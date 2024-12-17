using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusController : MonoBehaviour
{
    public GameManager gameManager;
    public LocationTracker locationTracker;
    public TimeManager timeManager;
    public PlayerStatusUI playerStatusUI;
    
    public float hp { get; private set; }
    public float oxygen { get; private set; }
    public float energy { get; private set; }

    public float maxHP { get; private set; } = 100f;
    public float maxOxygen { get; private set; } = 200f;
    public float maxEnergy { get; private set; } = 100f;

    private float hpUpdatePeriod = 6f; // in Region 3
    private float energyUpdatePeriod = 5f;
    private float oxygenUpdatePeriod = 1f;
    
    private float hpUpdateTimer = 0f;
    private float energyUpdateTimer = 0f;
    private float oxygenUpdateTimer = 0f;

    private float hpUpdateAmount = 4f;
    private float energyUpdateAmount = 2f;
    private float oxygenUpdateAmount = 1f;

    public bool isHeatProtected = false;
    
    // Start is called before the first frame update
    void Start()
    {
        // save & load -> from json
        hp = maxHP;
        oxygen = maxOxygen;
        energy = maxEnergy;
        gameManager.OnGameOver += GameOver;

    }

    private void Update()
    {
        if (gameManager.IsGameOver) return;
        
        if (locationTracker.currentRegionIndex == 2 && !timeManager.isNight)
        {
            if (!isHeatProtected)
            {
                hpUpdateTimer += Time.deltaTime;

                if (hpUpdateTimer > hpUpdatePeriod)
                {
                    hpUpdateTimer -= hpUpdatePeriod;
                    UpdateHP(-hpUpdateAmount);
                }
            }
        }
        
        if (!locationTracker.isBase)
        {
            oxygenUpdateTimer += Time.deltaTime;

            if (oxygenUpdateTimer > oxygenUpdatePeriod)
            {
                oxygenUpdateTimer -= oxygenUpdatePeriod;
                UpdateOxygen(-oxygenUpdateAmount);
            }
        }
        else
        {
            oxygen = maxOxygen;
        }

        energyUpdateTimer += Time.deltaTime;

        if (energyUpdateTimer > energyUpdatePeriod)
        {
            energyUpdateTimer -= energyUpdatePeriod;
            UpdateEnergy(-energyUpdateAmount);
        }
    }

    public void UpdateHP(float hpChange)
    {
        if (locationTracker.isBase && hpChange < 0) return;
        
        hp = Math.Max(hp + hpChange, 0);

        if (hp == 0)
        {
            gameManager.GameOver();
        }
        
    }

    public void UpdateEnergy(float energyChange)
    {
        energy = Math.Max(energy + energyChange, 0);

        if (energy == 0)
        {
            gameManager.GameOver();
        }
    }

    public void UpdateOxygen(float oxygenChange)
    {
        oxygen = Math.Max(oxygen + oxygenChange, 0);

        if (oxygen == 0)
        {
            gameManager.GameOver();
        }
    }

    public void UpgradeOxygen1()
    {
        maxOxygen += 100f;
        playerStatusUI.UpdateBarValues();
        Debug.Log($"maxOxygen is updated to {maxOxygen}");
    }

    public void UpgradeOxygen2()
    {
        maxOxygen += 100f;
        playerStatusUI.UpdateBarValues();
        Debug.Log($"maxOxygen is updated to {maxOxygen}");
    }

    void GameOver()
    {
        StartCoroutine(GameOverCoroutine());
    }

    IEnumerator GameOverCoroutine()
    {
        yield return new WaitForSeconds(gameManager.gameOverAnimationDuration);

        hp = maxHP / 2;
        oxygen = maxOxygen;
        energy = maxEnergy / 2;
    }
}
