using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public event Action OnGameOver;
    public bool IsGameOver = false;
    public bool IsGameClear = false;

    public float gameOverDuration { get; private set; } = 6f;
    public float gameOverAnimationDuration { get; private set; } = 2.5f;

    void Start()
    {

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
        yield return new WaitForSeconds(gameOverDuration);
        SceneManager.LoadScene("Closing"); // nextSceneName은 확장자 없이 Map 이런식으로만 해주면 됨. 단 해당 씬을 File>Build Settings>Scenes in Build에 등록해주어야 정상 동작
    }
}
