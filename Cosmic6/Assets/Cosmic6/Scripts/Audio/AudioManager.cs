using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] bgmClips; // 0,1,2 총 3개의 BGM
    private AudioSource audioSource;
    private int currentIndex = 0;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.15f;
        audioSource.playOnAwake = false;
        audioSource.loop = false; // loop를 끄고, 곡 끝날 때 다음 곡으로 넘어가도록
    }

    void Start()
    {
        if (bgmClips.Length > 0)
        {
            PlayCurrentBGM();
        }
        else
        {
            Debug.LogWarning("No BGM Clips assigned.");
        }
    }

    void Update()
    {
        // 재생중인 BGM이 있고, 더 이상 재생 중이지 않다면(즉 곡이 끝남)
        if (bgmClips.Length > 0 && !audioSource.isPlaying)
        {
            // 다음 곡 인덱스로 넘어감
            currentIndex = (currentIndex + 1) % bgmClips.Length;
            PlayCurrentBGM();
        }
    }

    private void PlayCurrentBGM()
    {
        audioSource.clip = bgmClips[currentIndex];
        audioSource.Play();
    }
}
