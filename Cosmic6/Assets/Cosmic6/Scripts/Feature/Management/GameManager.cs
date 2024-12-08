using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public event Action OnGameOver;
    public bool IsGameOver = false;

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
}
