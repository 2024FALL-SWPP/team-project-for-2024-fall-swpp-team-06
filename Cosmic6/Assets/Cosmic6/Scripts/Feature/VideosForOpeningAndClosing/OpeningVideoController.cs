using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class OpeningVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "Map";

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // 영상 재생이 끝나면 다음 씬 로드
        SceneManager.LoadScene(nextSceneName);
    }
}
