using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class OpeningVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    [Header("nextSceneName은 확장자 없이 Map 이런식으로만 해주면 됨. 단 해당 씬을 File>Build Settings>Scenes in Build에 등록해주어야 정상 동작")]
    public string nextSceneName = "Map_Coalesce_4am";

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
