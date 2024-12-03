using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffectManager : MonoBehaviour
{
    public GameManager gameManager;
    private Image darkScreen;
    private float fadeDuration;
    
    // Start is called before the first frame update
    void Start()
    {
        darkScreen = GetComponent<Image>();
        gameManager.OnGameOver += GameOver;
        fadeDuration = gameManager.gameOverAnimationDuration * 4 / 5;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GameOver()
    {
        StartCoroutine(FadeScreen());
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
}
