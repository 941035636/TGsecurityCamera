using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipUi : Window
{
    public TipPanel _MainTipPanel;
    public override void Awake(params object[] paralist)
    {
        _MainTipPanel = GameObject.GetComponent<TipPanel>();

    }
}
