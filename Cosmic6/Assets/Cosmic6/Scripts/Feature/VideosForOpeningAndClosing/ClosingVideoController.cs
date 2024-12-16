using UnityEngine;
using UnityEngine.Video;

public class ClosingVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // 영상 종료 시 게임 종료
        Application.Quit();

        // 에디터 환경에서 테스트용(에디터에서는 Quit 동작 안함)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#endif
    }
}
