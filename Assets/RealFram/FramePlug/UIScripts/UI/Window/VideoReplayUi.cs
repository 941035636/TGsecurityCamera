using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoReplayUi : Window
{
    public VideoReplayPanel _MainPeoplePanel;
    public override void Awake(params object[] paralist)
    {
        _MainPeoplePanel = GameObject.GetComponent<VideoReplayPanel>();

    }
}
