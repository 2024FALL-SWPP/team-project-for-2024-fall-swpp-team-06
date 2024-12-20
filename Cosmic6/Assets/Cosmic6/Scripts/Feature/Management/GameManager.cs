using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public event Action OnGameOver;
    public event Action OnGameStart;
    
    public bool IsGameOver = false;
    public bool IsGameClear = false;
    public bool IsGameStart = true;
    
    private readonly (int, int) targetAspect = (16, 9);

    public float gameOverDuration { get; private set; } = 6f;
    public float gameStartDuration { get; private set; } = 10.5f;
    public float gameOverAnimationDuration { get; private set; } = 2.5f;
    public float gameStartAnimationDuration { get; private set; } = 9.767f;

    void Start()
    {
        SetAspectRatio();
        
        if (IsGameStart)
        {
            OnGameStart?.Invoke();
            StartCoroutine(GameStartCoroutine());
        }
    }

    public void SetAspectRatio()
    {
        var width = Screen.width;
        var height = Screen.height;

        var unit = width / targetAspect.Item1;

        width = targetAspect.Item1 * unit;
        height = targetAspect.Item2 * unit;
        
        Screen.SetResolution(width, height, true);
    }

    IEnumerator GameStartCoroutine()
    {
        yield return new WaitForSeconds(gameStartDuration);
        IsGameStart = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GameOver()
    {
        IsGameOver = true;
        OnGameOver?.Invoke();
        StartCoroutine(GameOverCoroutine());
    }

    IEnumerator GameOverCoroutine()
    {
        yield return new WaitForSeconds(gameOverDuration);
        IsGameOver = false;
    }

    public void GameClear()
    {
        IsGameClear = true;
        StartCoroutine(GameClearCoroutine());
    }

    IEnumerator GameClearCoroutine()
    {
        // show ending scene
        Debug.Log("Game Clear!");
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene("Closing"); // nextSceneName은 확장자 없이 Map 이런식으로만 해주면 됨. 단 해당 씬을 File>Build Settings>Scenes in Build에 등록해주어야 정상 동작
    }
}
