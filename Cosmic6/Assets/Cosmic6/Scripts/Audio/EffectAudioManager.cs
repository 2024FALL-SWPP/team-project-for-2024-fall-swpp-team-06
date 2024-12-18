using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAudioManager : MonoBehaviour
{
    // Start is called before the first frame update
    private GameManager gameManager;
    private BaseManager baseManager;
    private TeleManager teleManager;
    private FlagManager flagManager;
    public PlantManager plantManager;

    public GameObject EffectAudioSource;
    private AudioSource effectAudioSource;
    public AudioClip gameOverAudio;
    public AudioClip baseRegisterAudio;
    public AudioClip teleFindAudio;
    public AudioClip harvestingAudio;
    public AudioClip tilingAudio;
    
    void Start()
    {
        effectAudioSource = EffectAudioSource.AddComponent<AudioSource>();
        effectAudioSource.playOnAwake = false;
        effectAudioSource.loop = false;
        gameManager = GetComponent<GameManager>();
        baseManager = GetComponent<BaseManager>();
        teleManager = GetComponent<TeleManager>();
        flagManager = GetComponent<FlagManager>();

        flagManager.OnFlagFound += TeleFoundSoundPlay;
        plantManager.OnHarvesting += HarvestingSoundPlay;
        gameManager.OnGameOver += GameoverSoundPlay;
        baseManager.OnBaseRegistered += BaseRegisterSoundPlay;
        teleManager.OnTeleFound += TeleFoundSoundPlay;
        FarmingManager.Instance.OnHarvesting += HarvestingSoundPlay;
        FarmingManager.Instance.OnTiling += TilingSoundPlay;
    }

    void GameoverSoundPlay()
    {
        effectAudioSource.clip = gameOverAudio;
        effectAudioSource.Play();
    }

    void BaseRegisterSoundPlay()
    {
        effectAudioSource.clip = baseRegisterAudio;
        effectAudioSource.Play();
    }

    public void TeleFoundSoundPlay()
    {
        effectAudioSource.clip = teleFindAudio;
        effectAudioSource.Play();
    }

    void HarvestingSoundPlay()
    {
        effectAudioSource.clip = harvestingAudio;
        effectAudioSource.Play();
    }

    void TilingSoundPlay()
    {
        effectAudioSource.clip = tilingAudio;
        effectAudioSource.Play();
    }
}

