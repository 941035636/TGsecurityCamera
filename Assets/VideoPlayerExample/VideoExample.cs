using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoExample : MonoBehaviour
{
    public string Path = "Video/test.mp4";
    private VideoPlayer player;
    public InputField PathText;
    private void Start()
    {
        player = FindObjectOfType<VideoPlayer>();
        player.OnOpenVideoFile(Path);
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            player.OnOpenVideoFile("Video/" + PathText.text);
        }
    }
}
