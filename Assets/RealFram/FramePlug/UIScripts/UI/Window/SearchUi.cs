using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchUi : Window
{
    public SearchPanel _SearchPanel;
    public override void Awake(params object[] paralist)
    {
        _SearchPanel = GameObject.GetComponent<SearchPanel>();

    }
}
