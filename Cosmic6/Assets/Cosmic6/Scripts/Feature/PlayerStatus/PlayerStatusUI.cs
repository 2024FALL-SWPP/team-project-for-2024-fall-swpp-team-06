using UnityEngine;

public class PlayerStatusUI : MonoBehaviour
{
    public ProgressBar hpProgressBar;
    public ProgressBar oxygenProgressBar;
    public ProgressBar energyProgressBar;
    
    public PlayerStatusController playerStatusController;
    /*
    private GameObject oxygen;
    private GameObject energy;
    private GameObject strength;

    private ProgressBar oxygenProgressBar;
    private ProgressBar energyProgressBar;
    private ProgressBar strengthProgressBar;
    */
    /*
    // Auto Increase/Decrease
    public bool oxygenAutoDecrease = true;
    public bool energyAutoDecrease = true;
    public bool strengthAutoIncrease = true;

    // Increase/Decrease Rate (per sec)
    public float oxygenDecreaseRate = 5f;
    public float energyDecreaseRate = 2f;
    public float strengthIncreaseRate = 1f;
    */
    void Start()
    {
        hpProgressBar.maxValue = playerStatusController.maxHP;
        oxygenProgressBar.maxValue = playerStatusController.maxOxygen;
        energyProgressBar.maxValue = playerStatusController.maxEnergy;
        SetBarValues();
    }

    void Update()
    {
        SetBarValues();
    }

    void SetBarValues()
    {
        hpProgressBar.SetValue(playerStatusController.hp);
        oxygenProgressBar.SetValue(playerStatusController.oxygen);
        energyProgressBar.SetValue(playerStatusController.energy);
    }
    
    /*
    void Start()
    {
        oxygen = GameObject.Find("Oxygen");
        energy = GameObject.Find("Energy");
        strength = GameObject.Find("Strength");

        oxygenProgressBar = oxygen.GetComponent<ProgressBar>();
        energyProgressBar = energy.GetComponent<ProgressBar>();
        strengthProgressBar = strength.GetComponent<ProgressBar>();

        oxygenProgressBar.SetValue(70f);
        energyProgressBar.SetValue(50f);
        strengthProgressBar.SetValue(50f);
    }

    void Update()
    {
        if (oxygenAutoDecrease)
        {
            DecreaseOxygen(Time.deltaTime * oxygenDecreaseRate);
        }

        if (energyAutoDecrease)
        {
            DecreaseEnergy(Time.deltaTime * energyDecreaseRate);
        }

        if (strengthAutoIncrease)
        {
            IncreaseStrength(Time.deltaTime * strengthIncreaseRate);
        }
    }

    public void DecreaseOxygen(float amount)
    {
        float newValue = Mathf.Max(0, oxygenProgressBar.BarValue - amount);
        oxygenProgressBar.SetValue(newValue);
    }

    public void DecreaseEnergy(float amount)
    {
        float newValue = Mathf.Max(0, energyProgressBar.BarValue - amount);
        energyProgressBar.SetValue(newValue);
    }

    public void IncreaseStrength(float amount)
    {
        float newValue = Mathf.Min(100, strengthProgressBar.BarValue + amount);
        strengthProgressBar.SetValue(newValue);
    }
    */
}
