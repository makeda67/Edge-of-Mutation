using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ForceVideoplayer : MonoBehaviour
{
    public RawImage targetImage;
    public VideoPlayer videoPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (targetImage != null)
            targetImage.enabled = false;

        videoPlayer.playOnAwake = false;

        videoPlayer.Prepare();

        videoPlayer.prepareCompleted += OnVideoPrepared;

    }

    // Update is called once per frame
    private void OnVideoPrepared(VideoPlayer vp)
    {
        if(targetImage != null)
            targetImage.enabled = true;
        
        vp.Play();
    }
}
