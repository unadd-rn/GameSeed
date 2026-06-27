using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Collections;

public class WebGLVideoFix : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string videoFileName = "your_video.webm";

    void Start()
    {
        // 1. Mute the video completely so the browser allows auto-play
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.playOnAwake = false;

        // 2. Start loading the video instantly on player load
        StartCoroutine(PrepareAndPlayWebGLVideo());
    }

    IEnumerator PrepareAndPlayWebGLVideo()
    {
        string videoPath = Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = videoPath;

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();
    }
}
