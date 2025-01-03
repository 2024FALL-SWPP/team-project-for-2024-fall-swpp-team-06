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

    public Action<int> Tutorial1Complete;
    private bool isStart;
    
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
        hp = 50;
        oxygen = maxOxygen;
        energy = 50;
        gameManager.OnGameOver += GameOver;
        isStart = gameManager.IsGameStart;

    }

    private void Update()
    {
        if (transform.position.y < -100)
        {
            UpdateHP(-hp);
        }
        
        if (gameManager.IsGameOver || gameManager.IsGameStart) return;
        
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
            UpdateEnergy(-energyUpdateAmount, false);
        }
    }

    public void UpdateHP(float hpChange)
    {
        if (locationTracker.isBase && hpChange < 0) return;
        
        hp = Math.Clamp(hp + hpChange, 0, maxHP);

        if (hp <= 0)
        {
            gameManager.GameOver();
        }
        
    }

    public void UpdateEnergy(float energyChange, bool isUsingItem)
    {
        energy = Math.Clamp(energy + energyChange, 0, maxEnergy);

        if (isStart && isUsingItem)
        {
            Tutorial1Complete?.Invoke(0);
            isStart = false;
        }

        if (energy <= 0)
        {
            gameManager.GameOver();
        }
    }

    public void UpdateOxygen(float oxygenChange)
    {
        oxygen = Math.Clamp(oxygen + oxygenChange, 0, maxOxygen);

        if (oxygen <= 0)
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
