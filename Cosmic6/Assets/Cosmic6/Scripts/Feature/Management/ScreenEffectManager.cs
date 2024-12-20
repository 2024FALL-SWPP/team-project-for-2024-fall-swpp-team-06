using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffectManager : MonoBehaviour
{
    public GameManager gameManager;
    public InstantMovement instantMovement;
    private Image darkScreen;
    private float fadeDuration;
    
    // Start is called before the first frame update
    private void Awake()
    {
        darkScreen = GetComponent<Image>();
        gameManager.OnGameOver += GameOver;
        gameManager.OnGameStart += GameStart;
        instantMovement.OnTeleport += Teleport;
        fadeDuration = gameManager.gameOverAnimationDuration * 4 / 5;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GameOver()
    {
        StartCoroutine(FadeScreen());
    }

    IEnumerator HalfFadeScreen()
    {
        darkScreen.enabled = true;
        var timer = 0f;

        while (timer < 4)
        {
            darkScreen.color = new(0, 0, 0, Mathf.Lerp(1f, 0f, timer / 4));
            timer += Time.deltaTime;
            yield return null;
        }

        darkScreen.enabled = false;
    }

    void GameStart()
    {
        StartCoroutine(HalfFadeScreen());
    }
    
    IEnumerator FadeScreen()
    {
        darkScreen.enabled = true;
        var timer = 0f;
        while (timer < fadeDuration)
        {
            darkScreen.color = new(0, 0, 0, Mathf.Lerp(0f, 1f, timer / fadeDuration));
            timer += Time.deltaTime;
            yield return null;
        }

        var waitTime = gameManager.gameOverDuration + 1 - 2 * fadeDuration - 0.1f / 6;

        yield return new WaitForSeconds(waitTime);

        timer = 0;

        while (timer < fadeDuration)
        {
            darkScreen.color = new(0, 0, 0, Mathf.Lerp(1f, 0f, timer / fadeDuration));
            timer += Time.deltaTime;
            yield return null;
        }

        darkScreen.enabled = false;
    }
    
    
    
    
    void Teleport()
    {
        StartCoroutine(FadeTeleportScreen());
    }

    IEnumerator FadeTeleportScreen()
    {
        darkScreen.enabled = true;
        var timer = 0f;
        while (timer < 1.0f)
        {
            darkScreen.color = new(0, 0, 0, Mathf.Lerp(0f, 1f, timer / 1.0f));
            timer += Time.deltaTime;
            yield return null;
        }

        var waitTime = 3.0f + 1 - 2 * 1.0f - 0.1f / 6;

        yield return new WaitForSeconds(waitTime);

        timer = 0;

        while (timer < 1.0f)
        {
            darkScreen.color = new(0, 0, 0, Mathf.Lerp(1f, 0f, timer / 1.0f));
            timer += Time.deltaTime;
            yield return null;
        }

        darkScreen.enabled = false;
    }
}
